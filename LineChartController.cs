using JDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AwesomeCharts;
using System.Globalization;
using System;
using System.Linq;
using UnityEditor;

public class LineChartController : MonoBehaviour
{
    [SerializeField]
    private Button currentButton;
    [SerializeField]
    private Sprite CurrentButtonHovered;
    [SerializeField]
    private Sprite CurrentButtonLight;
    [SerializeField]
    private Image _buttonImage;
    [SerializeField]
    private RectTransform LineChart;
    [SerializeField]
    private MainCameraController mainCameraControllerInstance;
    [SerializeField]
    private GameObject dataViewScrollsObject;
    [SerializeField]
    private LineChart lineChart;
    [SerializeField]
    private Text txt_CurrentValue, txt_ProposedValue;
    [SerializeField]
    private ToolTipBehavior toolTipBehaviorScript;

    [SerializeField]
    private WarningPanelBehavior warningPanel;

    public List<ShelfRenderTextureContainer> ShelfRenderTextures = new List<ShelfRenderTextureContainer>();

    [Serializable]
    public class ShelfRenderTextureContainer
    {
        public RawImage renderImage;
        public RectTransform renderImageTransform;
        public GameObject renderScrollView;
        public TextureRenderCamera UiRenderCamera;
        public Image ImpactHighlightImage;
        public Image StrategyNumberImage;
    }

    [SerializeField]
    private Sprite[] strategyNumberSprites;


    private Sprite _currentSprite;
    private decimal currentValue;
    private decimal proposedValue;


    [SerializeField]
    bool isLineChart;
    public bool IsLineChartModeActive
    {
        get
        {
            return isLineChart;
        }
    }

    public RectTransform canvasRect;
    List<LineEntry> currentEntries = new List<LineEntry>();
    Color currentPointColor = new Color32(234, 139, 160, 255);//#EA8BA0
    Color proposedPointColor = new Color32(1, 221, 205, 255);//#01DDCD

    //POSM Toggle Handler
    [SerializeField]
    private POSMHandler posmhandler;

    private void OnEnable()
    {
        if (_buttonImage == null)
        {
            _buttonImage = currentButton.GetComponent<Image>();
        }
        if (!currentButton.IsInteractable()) return;
       // _currentSprite = CurrentButtonLight;
      //  _buttonImage.sprite = _currentSprite;


        
    }


    public void MouseHovered()
    {
        if (!currentButton.IsInteractable()) return;
        _buttonImage.sprite = CurrentButtonHovered;

    }

    public void MouseExited()
    {
        if (!currentButton.IsInteractable()) return;
        _buttonImage.sprite = _currentSprite == null ? CurrentButtonLight : _currentSprite;
    }

    public void SetButtonHover(bool enabled)
    {
        _currentSprite = enabled ? CurrentButtonHovered : CurrentButtonLight;
        _buttonImage.sprite = _currentSprite;
    }

    public void ToggleLineChartButton()
    {
        SetLineChartEnabled(!isLineChart);
    }

    public void SetLineChartEnabled(bool enabled)
    {
       
        ModeManager.Instance.MoveFloatingNav(enabled);

        //if Compare Mode is active, disable it
        if (ModeManager.Instance.StrategycompareHandler.CompareMode)
        {
            ModeManager.Instance.StrategycompareHandler.DisableCompareMode();
        }

        isLineChart = enabled;
        SetButtonHover(enabled);
        EnableDisableLineChart(enabled);

        // UI should be opposite the state of this mode
        ModeManager.Instance.FairShareShortcutManager.ToggleUIElements(!enabled);

        if (enabled)
        {
            var mainCameraController = FindObjectOfType(typeof(MainCameraController)) as MainCameraController;
            // Disable camera zoom and pan 
            if (mainCameraController != null)
            {
                mainCameraController.cameraZoomEnabled = false;
                // Enable the shelfs before opening the mode in order for the cameras to frame themselves correctly
                mainCameraController.EnablingAllShelfs();
            }

            // When we turn this mode on, we want product selection to be disabled
            ModeManager.Instance.ProductSelectionManager.SetSelectionEnabled(false);
            if (!posmhandler.PosmAlways)
                AddedProductToggle.ToggleAddedShelfObjects(false);

        }
        else
        {
            //Disable compare (since Impact chart is considered a "compare"-esk format) 
            ModeManager.Instance.StrategycompareHandler.DisableCompareMode();

            ModeManager.Instance.StrategycompareHandler.SetCompareButtonInteractable(true);

            AddedProductToggle.ToggleAddedShelfObjects(true);
        }
    }

    /// <summary>
    /// Used for on hover of proposed or current value texts
    /// bool == true if you want txt_CurrentValue
    /// bool == false if you want txt_ProposedValue
    /// </summary>
    public void ShowValuesToolTips(bool isCurrentValue)
    {
        if (isCurrentValue) 
            toolTipBehaviorScript.ShowToolTip(CurrencyPreference.GetSelectedSymbol(currentValue));
        else 
            toolTipBehaviorScript.ShowToolTip(CurrencyPreference.GetSelectedSymbol(proposedValue));
    }
   
    /// <summary>
    /// handles Linechart toggling
    /// </summary>
    /// <param name="state"></param>
    /// <param name="onSwitch"></param>
    internal void EnableDisableLineChart(bool state)
    {
        LineChart.gameObject.SetActive(state);
        HandleStrategyButtons(!state);
        foreach (ShelfRenderTextureContainer scroll in ShelfRenderTextures)
        {
            scroll.renderScrollView.SetActive(state);
        }


        if (state)
        {
            StrategyManager.instance.UpdateActiveShelvesList();

            PlotLineGraph();
            dataViewScrollsObject.SetActive(!state);
            mainCameraControllerInstance.RightScreenUIElements.Add(LineChart);
            FairShareManager lassomanager = FindObjectOfType<FairShareManager>();
            lassomanager.MoveCameraToHideShelf();
            RenderShelfs();
            ResizeImpactGraph(ShelfRenderTextures);
          
        }
        else
        {
            ClearChart();
            DisableCameraComponents();
            mainCameraControllerInstance.RightScreenUIElements.Remove(LineChart);

            int closestShelfIndex = mainCameraControllerInstance.ClosestShelfIndex();
            mainCameraControllerInstance.CenterCameraOnShelf(closestShelfIndex, false);
        }

        //Setting Resolution of rendered images
        ShelfRenderTextures[0].renderImage.SetNativeSize();
        ShelfRenderTextures[1].renderImage.SetNativeSize();
    }

    public void PlotLineGraph()
    {
        //clear any previous entries
        currentEntries.Clear();
        ClearChart();

        if (!(PsaManager.PlanogramsSelectedForInteraction.Count > 1) || 
            (!XLSXDataChecker.PlanogramDataValid(StrategyManager.instance.ActiveShelves[0].shelfIndex) || 
                !XLSXDataChecker.PlanogramDataValid(StrategyManager.instance.ActiveShelves[1].shelfIndex)) )
        {
            // Show Warning Panel and Clear Chart
            warningPanel.OpenPanelWithText("No data in one or more strategies.");
            lineChart.SetDirty();

            txt_CurrentValue.text = "";
            txt_ProposedValue.text = "";
            return;
        }

        LineDataSet dataset = new LineDataSet();

        for (int i = 0; i < StrategyManager.instance.ActiveShelves.Count; i++)
        {
            int scenarioNumber = StrategyManager.instance.GetShelfStrategyNumber(StrategyManager.instance.ActiveShelves[i]);

            decimal POSSales = Math.Round(XLSXPSAImporter.GetDecimalSumOfValueForAllProducts("POSDollarSales", scenarioNumber));
            LineEntry entry = new LineEntry();
            entry.Position = i;
            entry.Value = POSSales;
            dataset.AddEntry(entry);
            currentEntries.Add(entry);
        }

        dataset.LineColor = proposedPointColor;//Line color from design
        lineChart.GetChartData().DataSets.Add(dataset);
        lineChart.SetDirty();
        //Let te indicators come to the list,to set the lables wait 0.1sec
        Invoke("LabelsOnLInechart", 0.1f);
    }


    /// <summary>
    /// Set Default value labels on line chart
    /// </summary>
    private void LabelsOnLInechart()
    {
        List<LineEntryIdicator> indicators = lineChart.entryIdicators;

        currentValue = currentEntries[0].Value;
        proposedValue = Math.Round(currentEntries[1].Value);

        //Set position of labels off from indicators (in a way that they don't overlap the line between them)
        txt_CurrentValue.transform.position = LabelPosition(indicators[0], true); 
        txt_ProposedValue.transform.position = LabelPosition(indicators[1], false);
  
        //Set the label for current's text to show value (using M shorthand)
        string convertedValue = ConvertToMValues(currentValue);
        string currencySymbol = CurrencyPreference.CurrencySymbol == "blank" ? "" : CurrencyPreference.CurrencySymbol;
        Debug.Log($"CValue : {convertedValue}");
        // Define the maximum character limit for the shelf name
        int maxNameLength = 27;

        // Get the shelf name and truncate if necessary
        string currentShelfName = StrategyManager.instance.ActiveShelves[0].ShelfGameObject.name;
        if (currentShelfName.Length > maxNameLength)
        {
            currentShelfName = currentShelfName.Substring(0, maxNameLength) + "..."; // Append "..." to indicate truncation
        }
        txt_CurrentValue.text = currentShelfName + "\n" + currencySymbol + convertedValue;

        //Set the label for current's text to show value (using M shorthand) plus percent Sales info 
        convertedValue = ConvertToMValues(proposedValue);
        double percentSales = (double)Math.Round((proposedValue - currentValue) / currentValue * 100);
        string arrow;
        if (percentSales > 0)
        {
            arrow = char.ConvertFromUtf32(0x2191);
        }
        else
        {
            arrow = char.ConvertFromUtf32(0x2193);
        }

        string proposedShelfName = StrategyManager.instance.ActiveShelves[1].ShelfGameObject.name;
        if (proposedShelfName.Length > maxNameLength)
        {
            proposedShelfName = proposedShelfName.Substring(0, maxNameLength) + "...";
        }

        txt_ProposedValue.text =  (proposedShelfName) + " (" + percentSales + "%" + arrow + ")" + "\n" + currencySymbol + convertedValue;
        //Set indicators to respective color
        indicators[0].image.color = currentPointColor;
        indicators[1].image.color = proposedPointColor;
    }

    /// <summary>
    /// Return the position a given label should be based on its indicator and which shelf it is
    /// </summary>
    /// <param name="indicator">which graph indicator this label pos for</param>
    /// <param name="current">whether this is the current or proposed (easy readability)</param>
    /// <returns></returns>
    Vector3 LabelPosition(LineEntryIdicator indicator, bool current)
    {
        Vector3 labelPos = new Vector3();
        Vector3 indicatorPos = indicator.transform.position;

        //Prevent the label from being positioned overlapping the line between the indicators by offsetting them appropriately 
        float labelOffset = current == (currentValue < proposedValue)? -(Screen.height / 24) : (Screen.height / 24);

        labelPos = new Vector3(indicatorPos.x, indicatorPos.y + labelOffset, indicatorPos.z);

        return labelPos;
    }

    /// <summary>
    /// Converts a number to only care about the biggest group of number places, and adds a 'M' on the end.
    /// </summary>
    string ConvertToMValues(float value)
    {
        float newValue = (float)Math.Round(value);
        // Give the value its number seperators (commas and decimal points)
        string convertedValue = newValue.ToString("N0");

        // Get the string of however many M's are needed based on how many commas we have
        string addedMs = new string('M', convertedValue.Count(f => (f == ',')));

        // Return the first group of number places + our M's, or IF we don't use any ,'s, then just return the whole number (addedMs is blank by default)
        return convertedValue.Substring(0, convertedValue.Contains(",") ? convertedValue.IndexOf(",") : convertedValue.Length) + addedMs;
    }

    /// <summary>
    /// Converts a number to only care about the biggest group of number places, and adds a 'M' on the end.
    /// </summary>
    string ConvertToMValues(decimal value)
    {
        decimal newValue = Math.Round(value);
        // Give the value its number seperators (commas and decimal points)
        string convertedValue = newValue.ToString("N0");

        // Get the string of however many M's are needed based on how many commas we have
        string addedMs = new string('M', convertedValue.Count(f => (f == ',')));

        Debug.LogError($"Value : {value}, Round : {newValue}, Converted : {convertedValue}");

        // Return the first group of number places + our M's, or IF we don't use any ,'s, then just return the whole number (addedMs is blank by default)
        return convertedValue.Substring(0, convertedValue.Contains(",") ? convertedValue.IndexOf(",") + 3 : convertedValue.Length) + addedMs;
    }

    void ClearChart()
    {
        lineChart.GetChartData().Clear();
    }
    /// <summary>
    /// Planogram wise UITexture Render
    /// </summary>
    void RenderShelfs()
    {
        if (!(PsaManager.PlanogramsSelectedForInteraction.Count > 1))
        {
            return;
        }

        for (int r = ShelfRenderTextures.Count - 1; r >= 0 ; r--)
        {
            OneTimeInstantiateRenderCams(ShelfRenderTextures[0].UiRenderCamera.gameObject, ShelfRenderTextures[r]);
            TextureRenderCamera renderCam = ShelfRenderTextures[r].UiRenderCamera;
            renderCam.RenderImage = ShelfRenderTextures[r].renderImage;
            renderCam.RenderImageRectTransform = ShelfRenderTextures[r].renderImageTransform;
            renderCam.HighlightImageTransform = ShelfRenderTextures[r].ImpactHighlightImage.GetComponent<RectTransform>();

            int strategyNumber = StrategyManager.instance.GetShelfStrategyNumber(StrategyManager.instance.ActiveShelves[r]);
            ShelfRenderTextures[r].StrategyNumberImage.sprite = strategyNumberSprites[strategyNumber];
            renderCam.RenderShelf(strategyNumber);
        }
        //Impact:current shelf outline
        ShelfRenderTextures[0].ImpactHighlightImage.color = currentPointColor;
        //Impact:proposed shelf outline
        ShelfRenderTextures[1].ImpactHighlightImage.color = proposedPointColor;
    }


    /// <summary>
    /// Handles to not to instantiate UiRenderCamera's on every click
    /// </summary>
    /// <param name="uiCam"></param>
    /// <param name="instance"></param>
    void OneTimeInstantiateRenderCams(GameObject uiCam, ShelfRenderTextureContainer instance)
    {
        var rendercams = FindObjectsOfType<TextureRenderCamera>();
        if (rendercams.Length < 2)
        {
            GameObject uiRenderCam = Instantiate(uiCam);
            instance.UiRenderCamera = uiRenderCam.GetComponent<TextureRenderCamera>();
        }
    }
    //Make sure to turnoff the cameras when we are not rendering.
    void DisableCameraComponents()
    {
        for (int r = 0; r < ShelfRenderTextures.Count; r++)
        {
            //Always keep one UIcamera in the scene
            if (r == 0)
            {
                TextureRenderCamera renderCam = ShelfRenderTextures[r].UiRenderCamera;
                renderCam.GetComponent<Camera>().enabled = false;
            }
            else//Destroy the cloned cameras
            {
                Destroy(ShelfRenderTextures[r].UiRenderCamera.gameObject);
            }

        }
    }

    void ResizeImpactGraph(List<ShelfRenderTextureContainer> textures)
    {
        //Total width of texture parent which is occupying the canvas space
        float totalWidth = (textures[0].renderScrollView.GetComponent<RectTransform>().rect.width + textures[1].renderScrollView.GetComponent<RectTransform>().rect.width) / 2;
        float dynamicSize = totalWidth / (canvasRect.rect.width);
        dynamicSize = Mathf.Clamp(dynamicSize, .15f, .55f);
        //calculate offset to maintain the texture to fit on its parent
        float offSetX = (textures[0].renderScrollView.GetComponent<RectTransform>().anchorMax.x - textures[0].renderScrollView.GetComponent<RectTransform>().anchorMin.x - dynamicSize) * ((canvasRect.rect.width));
        foreach (ShelfRenderTextureContainer texture in textures)
        {
            texture.renderImageTransform.offsetMin = new Vector2(offSetX, texture.renderImageTransform.offsetMin.y);
            texture.renderImageTransform.offsetMax = new Vector2(-offSetX, texture.renderImageTransform.offsetMax.y);

        }
        //Make the same offset for line chart to fit from right
        LineChart.offsetMin = new Vector3(offSetX, LineChart.offsetMax.y);
        LineChart.offsetMax = new Vector3(offSetX, LineChart.offsetMax.y);
    }

    public static void HandleStrategyButtons(bool state, bool shortcutsOnly = false)
    {
        FairShareManager _lassoManager = FindObjectOfType<FairShareManager>();
        _lassoManager.EnableDisableCenterStrategyOptions(state, shortcutsOnly);
    }
}

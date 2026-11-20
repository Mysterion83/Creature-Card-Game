using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider masterSlider, musicSlider, sfxSlider, uiSlider;
    [SerializeField] TMP_Text masterValue, musicValue, sfxValue, uiValue;

    [Header("Graphics")]
    [SerializeField] TMP_Dropdown displayModeDropdown, resolutionDropdown, qualityDropdown;
    [SerializeField] Toggle vsyncToggle, aaToggle, shadowsToggle, postToggle;

    [Header("Save / Load")]
    [SerializeField] SaveSlotView[] slots;
    [SerializeField] Button saveButton, loadButton;
    [SerializeField] MenuController menu;
    [SerializeField] GameObject levelSelectPanel;
    [SerializeField] Popup popup;

    static readonly FullScreenMode[] Modes =
        { FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen, FullScreenMode.Windowed };

    readonly List<Resolution> resolutions = new();
    bool built;
    int selectedSlot = -1;

    void Awake()
    {       
        masterSlider.onValueChanged.AddListener(v => masterValue.text = Percent(v));
        musicSlider.onValueChanged.AddListener(v => musicValue.text = Percent(v));
        sfxSlider.onValueChanged.AddListener(v => sfxValue.text = Percent(v));
        uiSlider.onValueChanged.AddListener(v => uiValue.text = Percent(v));

        foreach (var view in slots) view.Clicked += OnSlotClicked;
    }

    void OnEnable()
    {
        BuildDropdowns();
        LoadIntoControls();
        RefreshSlots();
    }

    // Buttons 
    public void Apply()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetFloat("UIVolume", uiSlider.value);

        PlayerPrefs.SetInt("DisplayMode", displayModeDropdown.value);
        Resolution r = resolutions[resolutionDropdown.value];
        PlayerPrefs.SetInt("ResW", r.width);
        PlayerPrefs.SetInt("ResH", r.height);
        PlayerPrefs.SetInt("Quality", qualityDropdown.value);

        PlayerPrefs.SetInt("VSync", vsyncToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("AA", aaToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Shadows", shadowsToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Post", postToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplySaved(mixer);
    }

    public void RestoreDefaults()  
    {
        SetSlider(masterSlider, masterValue, 0.8f);
        SetSlider(musicSlider, musicValue, 0.7f);
        SetSlider(sfxSlider, sfxValue, 0.8f);
        SetSlider(uiSlider, uiValue, 0.9f);

        displayModeDropdown.SetValueWithoutNotify(0);
        resolutionDropdown.SetValueWithoutNotify(resolutions.Count - 1);
        qualityDropdown.SetValueWithoutNotify(QualitySettings.names.Length - 1);

        vsyncToggle.SetIsOnWithoutNotify(true);
        aaToggle.SetIsOnWithoutNotify(true);
        shadowsToggle.SetIsOnWithoutNotify(true);
        postToggle.SetIsOnWithoutNotify(true);
    }

    public void SaveGame()
    {
        ProfileManager.Instance.Save();
        RefreshSlots();
    }

    public void LoadGame()
    {
        if (selectedSlot < 0 || !ProfileManager.Instance.LoadGame(selectedSlot)) return;
        popup.Hide();
        menu.OpenFresh(levelSelectPanel);
    }

    // Applying (also used once at game start)
    public static void ApplySaved(AudioMixer mixer)
    {
        if (mixer != null)
        {
            SetVolume(mixer, "MasterVolume", PlayerPrefs.GetFloat("MasterVolume", 0.8f));
            SetVolume(mixer, "MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 0.7f));
            SetVolume(mixer, "SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 0.8f));
            SetVolume(mixer, "UIVolume", PlayerPrefs.GetFloat("UIVolume", 0.9f));
        }

        // Quality first: each quality level swaps in its own URP asset
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel()), true);
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSync", 1) == 1 ? 1 : 0;

        FullScreenMode mode = Modes[Mathf.Clamp(PlayerPrefs.GetInt("DisplayMode", 0), 0, Modes.Length - 1)];
        int w = PlayerPrefs.GetInt("ResW", Screen.currentResolution.width);
        int h = PlayerPrefs.GetInt("ResH", Screen.currentResolution.height);
        Screen.SetResolution(w, h, mode);

        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
        {
            urp.msaaSampleCount = PlayerPrefs.GetInt("AA", 1) == 1 ? 4 : 1;
            urp.shadowDistance = PlayerPrefs.GetInt("Shadows", 1) == 1 ? 50f : 0f;
        }

        if (Camera.main != null)
            Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = PlayerPrefs.GetInt("Post", 1) == 1;
    }

    static void SetVolume(AudioMixer mixer, string param, float value) =>
        mixer.SetFloat(param, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);

    // Filling the controls
    void BuildDropdowns()
    {
        if (built) return;
        built = true;

        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new List<string> { "Fullscreen", "Exclusive Fullscreen", "Windowed" });

        resolutions.Clear();
        var labels = new List<string>();
        foreach (Resolution r in Screen.resolutions)
        {
            if (resolutions.Exists(x => x.width == r.width && x.height == r.height)) continue;
            resolutions.Add(r);
            labels.Add($"{r.width} x {r.height}");
        }
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    void LoadIntoControls()
    {
        SetSlider(masterSlider, masterValue, PlayerPrefs.GetFloat("MasterVolume", 0.8f));
        SetSlider(musicSlider, musicValue, PlayerPrefs.GetFloat("MusicVolume", 0.7f));
        SetSlider(sfxSlider, sfxValue, PlayerPrefs.GetFloat("SFXVolume", 0.8f));
        SetSlider(uiSlider, uiValue, PlayerPrefs.GetFloat("UIVolume", 0.9f));

        displayModeDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("DisplayMode", 0));

        int w = PlayerPrefs.GetInt("ResW", Screen.currentResolution.width);
        int h = PlayerPrefs.GetInt("ResH", Screen.currentResolution.height);
        int index = resolutions.FindIndex(r => r.width == w && r.height == h);
        resolutionDropdown.SetValueWithoutNotify(index >= 0 ? index : resolutions.Count - 1);

        qualityDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel()));

        vsyncToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("VSync", 1) == 1);
        aaToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("AA", 1) == 1);
        shadowsToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Shadows", 1) == 1);
        postToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Post", 1) == 1);
    }

    void RefreshSlots()
    {
        foreach (var view in slots) view.Refresh(false);
        selectedSlot = -1;
        loadButton.interactable = false;
        saveButton.interactable = ProfileManager.Instance != null && ProfileManager.Instance.ActiveSlot >= 0;
    }

    void OnSlotClicked(SaveSlotView view)
    {
        selectedSlot = view.Slot;
        loadButton.interactable = view.HasSave;
    }

    static void SetSlider(Slider slider, TMP_Text label, float value)
    {
        slider.SetValueWithoutNotify(value);
        label.text = Percent(value);
    }

    static string Percent(float v) => $"{Mathf.RoundToInt(v * 100)}%";
}
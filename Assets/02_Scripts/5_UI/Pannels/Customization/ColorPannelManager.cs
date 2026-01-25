using System.Collections.Generic;
using UnityEngine;

public class ColorPannelManager : MonoBehaviour
{
    [Header("Project References")]
    [SerializeField] private CustomizationData_SO customizationData_SO;
    [SerializeField] private PlayerSkinData_SO playerSkinData_SO;

    [Header("Scene References")]
    [SerializeField] private PlayerCustomization playerCustomization;
    [SerializeField] private GameObject colorTab;
    [SerializeField] private GameObject colorSlotContainer;
    [SerializeField] private GameObject colorSlotPrefab;

    [Header("Runtime References")]
    [SerializeField] private CustomizationOption selectedOption;

    private void OnEnable()
    {
        CustomizationManager.Instance.OnOptionChanged += SetSelectedOption;
    }
    private void OnDisable()
    {
        CustomizationManager.Instance.OnOptionChanged -= SetSelectedOption;
    }


    private void Start()
    {
        for (int i = 1; i < customizationData_SO.colors.Length; i++)
        {
            GameObject colorSlot = Instantiate(colorSlotPrefab, colorSlotContainer.transform);
            colorSlot.GetComponent<ColorSlot>().InitializeColorSlot(customizationData_SO.colors[i], i, playerCustomization);
        }
        SetTabActivationAccordingToOption(playerSkinData_SO.skinOption);
    }

    private void SetSelectedOption(CustomizationSlot slot)
    {
        selectedOption = slot.option;
        SetTabActivationAccordingToOption(selectedOption);
    }

    private void SetTabActivationAccordingToOption(CustomizationOption option)
    {
        if (selectedOption is SkinOption skinOption)
        {
            bool isTabActive = !skinOption.isPremium;
            colorTab.SetActive(isTabActive);
        }
    }
}

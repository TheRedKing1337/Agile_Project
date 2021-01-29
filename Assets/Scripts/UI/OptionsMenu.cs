using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
   public AudioMixer AudioMixer;

   public Dropdown resolutionDropdown;
   private Resolution[] _resolutions;
   
   public GameObject mainMenu;
   public GameObject optionsMenu;
   
   // Is called on start
   // Clear all the options in the resolution dropdown
   // Create a list of strings which is going to be our options
   // Loop through each element in _resolutions array
   // For each of them create a nicely formatted string and add it to the options list
   // When done, add resolutions list to dropdown
   private void Start()
   {
      _resolutions = Screen.resolutions;
      
      resolutionDropdown.ClearOptions();

      List<string> options = new List<string>();

      int currentResolutionIndex = 0;
      for (int i = 0; i < _resolutions.Length; i++)
      {
         string option = _resolutions[i].width + "X" + _resolutions[i].height;
         options.Add(option);

         if (_resolutions[i].width == Screen.currentResolution.width && _resolutions[i].height == Screen.currentResolution.height)
         {
            currentResolutionIndex = i;
         }
      }
      
      resolutionDropdown.AddOptions(options);
      resolutionDropdown.value = currentResolutionIndex;
      resolutionDropdown.RefreshShownValue();
   }

   public void SetResolution(int resolutionIndex)
   {
      Resolution resolution = _resolutions[resolutionIndex];
      Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
   }
   // Hooks the audiomixer to the slider
   public void SetVolume(float volume)
   {
      AudioMixer.SetFloat("MasterVolume", volume);
   }

   // Sets the qualitylevel
   public void SetQuality(int qualityIndex)
   {
      QualitySettings.SetQualityLevel(qualityIndex);
   }

   // Sets the fullscreen
   public void SetFullscreen(bool isFullscreen)
   {
      Screen.fullScreen = isFullscreen;
   }

   public void BackButton()
   {
      optionsMenu.SetActive(false);
      mainMenu.SetActive(true);
   }
}

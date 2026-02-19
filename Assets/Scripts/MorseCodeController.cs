using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

public class MorseCodeController : MonoBehaviour
{
    public Action<string> OnMorseCodeSymbolRegistered;      // ./-
    public Action<string> OnMorseCodeCharacterRegistered;   // alphabet/numbers
    
    [Title("Timing Settings")]
    [Range(0.2f, 2f)]
    public float characterGapThreshold = 1.0f;

    [Title("UI")]
    [Required] public Slider timeSlider;

    [Required] public TextMeshProUGUI messageText;
    
    [ShowInInspector, ReadOnly]
    private float timeSinceLastInput;

    [ShowInInspector, ReadOnly]
    private string currentMorseCharacter = "";

    [ShowInInspector, ReadOnly]
    private string currentMessage = "";

    public event Action<string> OnMessageFinished;

    public bool isActive = false;
    
    private float lastInputTime;
    private bool hasInput;

    private readonly Dictionary<string, char> morseToChar = new Dictionary<string, char>()
    {
        { "·-", 'A' },   { "-···", 'B' }, { "-·-·", 'C' }, { "-··", 'D' },
        { "·", 'E' },    { "··-·", 'F' }, { "--·", 'G' },  { "····", 'H' },
        { "··", 'I' },   { "·---", 'J' }, { "-·-", 'K' },  { "·-··", 'L' },
        { "--", 'M' },   { "-·", 'N' },   { "---", 'O' },  { "·--·", 'P' },
        { "--·-", 'Q' }, { "·-·", 'R' },  { "···", 'S' },  { "-", 'T' },
        { "··-", 'U' },  { "···-", 'V' }, { "·--", 'W' },  { "-··-", 'X' },
        { "-·--", 'Y' }, { "--··", 'Z' }
    };

    private void Update()
    {
        if (!hasInput)
            return;

        timeSinceLastInput = Time.time - lastInputTime;

        if (timeSlider != null)
            timeSlider.value = Mathf.Clamp01(timeSinceLastInput / characterGapThreshold);

        if (timeSinceLastInput >= characterGapThreshold)
            RegisterCharacter();
    }

    [Button(ButtonSizes.Large)]
    public void Dot()
    {
        if (!isActive) return;
        AudioManager.Instance?.PlayOneShot("dot");
        RegisterInput("·");
    }

    [Button(ButtonSizes.Large)]
    public void Dash()
    {
        if (!isActive) return;
        AudioManager.Instance?.PlayOneShot("dash");
        RegisterInput("-");
    }

    private void RegisterInput(string symbol)
    {
        OnMorseCodeSymbolRegistered?.Invoke(symbol);
        currentMorseCharacter += symbol;
        lastInputTime = Time.time;
        timeSinceLastInput = 0f;
        hasInput = true;

        if (timeSlider != null)
            timeSlider.value = 0f;
    }

    private void RegisterCharacter()
    {
        if (morseToChar.TryGetValue(currentMorseCharacter, out char character))
        {
            OnMorseCodeCharacterRegistered?.Invoke(character.ToString());
            currentMessage += character;
        }

        currentMorseCharacter = "";
        hasInput = false;
        timeSinceLastInput = 0f;

        if (timeSlider != null)
            timeSlider.value = 0f;
        
        messageText.text = currentMessage;
    }

    [Button(ButtonSizes.Large)]
    public void FinishMessage()
    {
        if (hasInput && !string.IsNullOrEmpty(currentMorseCharacter))
            RegisterCharacter();

        OnMessageFinished?.Invoke(currentMessage);
        messageText.text = "";
        currentMessage = "";
    }

    [Button]
    public void Clear()
    {
        messageText.text = "";
        currentMorseCharacter = "";
        currentMessage = "";
        hasInput = false;
        timeSinceLastInput = 0f;

        if (timeSlider != null)
            timeSlider.value = 0f;
    }

}

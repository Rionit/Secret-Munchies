using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Messages", menuName = "Dialogue/Messages")]
public class DialogueMessageSO : ScriptableObject
{
    public enum MessageType
    {
        OPENER,
        ORDER,
        CHIT_CHAT,
        TOP_SECRET
    }

    [Title("Dialogue Type")]
    [EnumToggleButtons]
    public MessageType type;

    [Title("Variations")]
    [InfoBox("One variation is chosen randomly. Each variation is a sequence of messages.")]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [ShowIf(nameof(HasText))]
    public List<DialogueVariation> variations = new();

    public IEnumerable<string> GetRandomVariation(List<FoodAmount> wantedFoods = null)
    {
        if (type == MessageType.ORDER)
        {
            Debug.Log(GenerateOrderText(wantedFoods));
            yield return GenerateOrderText(wantedFoods);
            yield break;
        }
        
        if (variations == null || variations.Count == 0)
            yield break;

        DialogueVariation variation =
            variations[Random.Range(0, variations.Count)];


        foreach (var msg in variation.sequence)
            yield return msg;
    }

    private string GenerateOrderText(List<FoodAmount> foods)
    {
        if (foods == null || foods.Count == 0)
            return "Uh... actually never mind.";

        List<string> parts = new();

        foreach (var fa in foods)
        {
            string foodName = fa.food.GetDisplayName(fa.amount);
            parts.Add($"{fa.amount} {foodName}");
        }

        if (parts.Count == 1)
            return parts[0];
        else if (parts.Count == 2)
            return $"{parts[0]} and {parts[1]}";
        else
            return string.Join(", ", parts.Take(parts.Count - 1)) + " and " + parts.Last();
    }
    
    private void OnValidate()
    {
        if (variations == null) return;

        foreach (var v in variations)
        {
            if (v != null)
                v.parent = this;
        }
    }

    private bool HasText() => type != MessageType.ORDER;
}
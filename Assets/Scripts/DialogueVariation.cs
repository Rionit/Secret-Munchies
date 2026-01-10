using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[System.Flags]
public enum TopSecretCategory : byte
{
    None     = 0,
    Aliens   = 1 << 0,
    BlackOps = 1 << 1,
    Politics = 1 << 2,
    War      = 1 << 3,
}

[System.Serializable]
public class DialogueVariation
{
    [HideInInspector]
    public DialogueMessageSO parent;

    [FormerlySerializedAs("messages")]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [InfoBox(
        "There has to be at least one message!",
        InfoMessageType.Error,
        VisibleIf = nameof(HasMessage))]
    public List<string> sequence = new();

    [Title("Top Secret Categories")]
    [ShowIf(nameof(IsTopSecret))]
    [InfoBox(
        "Invalid combination!\nAllowed:\nA B C D\nAB BC CD AD\nABC BCD ACD ABD\nABCD",
        InfoMessageType.Error,
        VisibleIf = nameof(IsInvalidCombination))]
    [EnumToggleButtons]
    public TopSecretCategory categories = TopSecretCategory.None;

    private bool IsTopSecret()
    {
        return parent != null &&
               parent.type == DialogueMessageSO.MessageType.TOP_SECRET;
    }

    private bool HasMessage()
    {
        return sequence == null || sequence.Count == 0;
    }

    private bool Has(TopSecretCategory flag)
    {
        return (categories & flag) != 0;
    }

    private bool IsInvalidCombination()
    {
        if (!IsTopSecret()) return false;

        int count = Enum
            .GetValues(typeof(TopSecretCategory))
            .Cast<TopSecretCategory>()
            .Count(f => f != TopSecretCategory.None && Has(f));

        if (count == 1 || count >= 3)
            return false;

        return !(
            Has(TopSecretCategory.Aliens | TopSecretCategory.BlackOps) ||
            Has(TopSecretCategory.BlackOps | TopSecretCategory.Politics) ||
            Has(TopSecretCategory.Politics | TopSecretCategory.War) ||
            Has(TopSecretCategory.Aliens | TopSecretCategory.War)
        );
    }
}

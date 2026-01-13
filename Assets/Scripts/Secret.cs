using System;

public enum SecretState
{
    Active,
    Correct,
    Wrong
}

[Serializable]
public class Secret
{
    public TopSecretCategory category;
    public SecretState state;

    public Secret(TopSecretCategory category)
    {
        this.category = category;
        this.state = SecretState.Active;
    }
}
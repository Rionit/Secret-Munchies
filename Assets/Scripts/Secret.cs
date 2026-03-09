using System;

[System.Serializable]
public class SecretMessageMapping
{
    public string message;
    public TopSecretCategory category;
}

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
    
    public static TopSecretCategory GetSecretCategory(string message)
    {
        return message.ToLower() switch
        {
            "thereisastarman"     => TopSecretCategory.Aliens,
            "erasethenapkin"        => TopSecretCategory.BlackOps,
            "bringextrasocks"       => TopSecretCategory.Politics,
            "theweekislong"        => TopSecretCategory.War,

            "catknowsmuch"          => TopSecretCategory.Aliens | TopSecretCategory.BlackOps,
            "wrongdooragain"        => TopSecretCategory.BlackOps | TopSecretCategory.Politics,
            "smileandnod"           => TopSecretCategory.Politics | TopSecretCategory.War,
            "donotmicrowave"        => TopSecretCategory.Aliens | TopSecretCategory.War,

            "letitbe"               => TopSecretCategory.Aliens | TopSecretCategory.BlackOps | TopSecretCategory.Politics,
            "chickenoutofcoop"     => TopSecretCategory.BlackOps | TopSecretCategory.Politics | TopSecretCategory.War,
            "burnthewitch"          => TopSecretCategory.Aliens | TopSecretCategory.Politics | TopSecretCategory.War,
            "fishingforfishies"     => TopSecretCategory.Aliens | TopSecretCategory.BlackOps | TopSecretCategory.War,

            "everythingisnormal"    => TopSecretCategory.Aliens | TopSecretCategory.BlackOps | TopSecretCategory.Politics | TopSecretCategory.War,

            _ => TopSecretCategory.None
        };
    }
}
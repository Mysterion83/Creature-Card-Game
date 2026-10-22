using System;
using System.Collections.Generic;

[Serializable]
public class UserData
{
    public float Gold;
    public List<Card> Collection;
    public int LevelUnlocked;

    [Obsolete("Placeholder")]
    public int Settings;
}

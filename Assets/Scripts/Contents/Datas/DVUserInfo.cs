using UnityEngine;

[System.Serializable]
public struct DVUserInfo
{
    public string ID; // 특수문자 X
    public string Password; // 해쉬 코드
    public string Email;
    public string NickName;
}

using UnityEngine;

public abstract class BaseCharacter : ScriptableObject
{
    [Header("Character Info")]
    [SerializeField] private string _characterName;
    [SerializeField] private string _characterDescription;

    public Sprite charImage;
    public string characterName => _characterName;
    public string characterDescription => _characterDescription;
    public int health = 15;
    public bool skillChecked = false;                      //De dam bao chi kich hoat ky nang 1 lan duy nhat trong 1 vong dau
    public bool isDead = false;                            //De dam bao nhan vat chi chet 1 lan duy nhat trong 1 vong dau
    public bool canActivateSkill;

    public BaseCharacter() { }
    public abstract bool ApplySkills(BaseCharacter target, GameManager gameManager, GameStateManager gameStateManager); //Moi nhan vat se co 1 ky nang rieng biet, duoc kich hoat ơ trong Main Phase
    
    public  abstract void TakeDamage(int damage);
   
    public bool isDeadState => isDead;
    public void Reset()
    {
        health = 15;
        skillChecked = false;
        isDead = false;
        canActivateSkill = true;
    }
}

using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal enum StatsType
{
    Health,
    Speed,
    TotalDamage
}

internal enum Abilities
{
    Invincibility,
    Shield,
    Fear
}

internal enum NoAbilitiesRemaining
{
    HideCard,
    ShowTwoStatsCards,
}

[System.Serializable]
internal class StatsOption
{
    [TableColumnWidth(64, Resizable = false)]
    [PreviewField(Alignment = ObjectFieldAlignment.Center)]
    public Sprite image;
    [TextArea]
    public string text;
    public StatsType statsType;
    [EnableIf("isRandom")]
    [TableColumnWidth(200, Resizable = false)]
    [MinValue(1f)]
    public Vector2 randomStatMultiplier = new(1, 20);
    [DisableIf("isRandom")]
    [MinValue(1f)]
    public float statMultiplier = 1;
    public bool isRandom = true;
}

[System.Serializable]
internal class AbilityOption
{
    [TableColumnWidth(64, Resizable = false)]
    [PreviewField(Alignment = ObjectFieldAlignment.Center)]
    public Sprite image;
    [TextArea]
    public string text;
    public Abilities ability;
}

public class ShrinesUI : MonoBehaviour
{
    [TabGroup("Settings")]
    [TableList(ShowPaging = true)]
    [SerializeField]
    private List<StatsOption> statsCards;
    [TabGroup("Settings")]
    [TableList]
    [SerializeField]
    [RequiredListLength(null, "@Enum.GetNames(typeof(Abilities)).Length")]
    private List<AbilityOption> abilitiesCards;
    [TabGroup("Settings")]
    [SerializeField]
    [EnumToggleButtons]
    private NoAbilitiesRemaining ifNoAbilitiesRemaining;
    [SerializeField]
    [TabGroup("References")]
    private Image option1Image;
    [SerializeField]
    [TabGroup("References")]
    private Image option2Image;
    [SerializeField]
    [TabGroup("References")]
    private TextMeshProUGUI option1Text;
    [SerializeField]
    [TabGroup("References")]
    private TextMeshProUGUI option2Text;
    [TabGroup("References")]
    [SerializeField]
    private GameObject card1;
    [TabGroup("References")]
    [SerializeField]
    private GameObject card2;

    private Queue<AbilityOption> _remainingAbilities = new();
    private StatsOption _currentStatsOption;
    private float _currentStatMultiplier;
    private StatsOption _currentStats2Option;
    private float _currentStat2Multiplier;
    private AbilityOption _currentAbilityOption;
    private Shrine _currentShrine;

    private void Start()
    {
        for (int i = abilitiesCards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (abilitiesCards[i], abilitiesCards[j]) = (abilitiesCards[j], abilitiesCards[i]);
        }

        foreach (var item in abilitiesCards)
        {
            _remainingAbilities.Enqueue(item);
        }

        option1Image.preserveAspect = true;
        option2Image.preserveAspect = true;

        HideUI();
    }


    public void ShowUI(Shrine shrine)
    {
        _currentShrine = shrine;
        _currentStatsOption = statsCards[Random.Range(0, statsCards.Count)];
        if (_currentStatsOption.isRandom)
        {
            _currentStatMultiplier = Random.Range(_currentStatsOption.randomStatMultiplier.x, _currentStatsOption.randomStatMultiplier.y);
        }
        else
        {
            _currentStatMultiplier = _currentStatsOption.statMultiplier;
        }

        if (_remainingAbilities.Count == 0)
        {
            if (ifNoAbilitiesRemaining == NoAbilitiesRemaining.ShowTwoStatsCards)
            {
                _currentStats2Option = statsCards[Random.Range(0, statsCards.Count)];
                if (_currentStats2Option.isRandom)
                {
                    _currentStat2Multiplier = Random.Range(_currentStats2Option.randomStatMultiplier.x, _currentStats2Option.randomStatMultiplier.y);
                }
                else
                {
                    _currentStat2Multiplier = _currentStats2Option.statMultiplier;
                }
                option2Image.sprite = _currentStats2Option.image;
                option2Text.text = _currentStats2Option.text.Replace("{value}", Mathf.FloorToInt((_currentStat2Multiplier - 1) * 100).ToString());
                card2.SetActive(true);
            }
        }
        else
        {
            _currentAbilityOption = _remainingAbilities.Dequeue();
            option2Image.sprite = _currentAbilityOption.image;
            option2Text.text = _currentAbilityOption.text;
            card2.SetActive(true);
        }

        option1Image.sprite = _currentStatsOption.image;
        option1Text.text = _currentStatsOption.text.Replace("{value}", Mathf.FloorToInt((_currentStatMultiplier - 1) * 100).ToString());
        card1.SetActive(true);
    }

    private void HideUI()
    {
        card1.SetActive(false);
        card2.SetActive(false);
        option1Image.sprite = null;
        option1Text.text = "";
        option2Image.sprite = null;
        option2Text.text = "";
    }

    public void Option1Pressed()
    {
        HideUI();
        if (_currentAbilityOption != null && _currentStats2Option == null)
        {
            _remainingAbilities.Enqueue(_currentAbilityOption);
        }
        HandleStatsUpgrade(_currentStatsOption, _currentStatMultiplier);
        _currentShrine.onShrineActivated.Invoke();
    }

    public void Option2Pressed()
    {
        HideUI();
        if (_currentStats2Option != null)
        {
            HandleStatsUpgrade(_currentStats2Option, _currentStat2Multiplier);
        }
        else
        {
            switch (_currentAbilityOption.ability)
            {
                case Abilities.Shield:
                    Debug.Log("Shield Activated (not implemented)");
                    break;
                case Abilities.Fear:
                    Debug.Log("Fear Activated (not implemented)");
                    break;
                case Abilities.Invincibility:
                    Debug.Log("Invincibility Activated (not implemented)");
                    break;
            }
        }
        _currentShrine.onShrineActivated.Invoke();
    }

    private void HandleStatsUpgrade(StatsOption statsOption, float multiplier)
    {
        var player = GameManager.Instance._player;
        switch (statsOption.statsType)
        {
            case StatsType.Health:
                player.GetComponent<Health>().MultiplyMaxHealth(multiplier);
                Debug.Log("Health Increased");
                break;
            case StatsType.Speed:
                player.GetComponent<PlayerController>().speed *= multiplier;
                Debug.Log("Speed Increased");
                break;
            case StatsType.TotalDamage:
                GameManager.Instance.damageMultiplier *= multiplier;
                break;
        }
    }
}
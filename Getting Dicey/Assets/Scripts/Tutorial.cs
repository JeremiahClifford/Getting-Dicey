using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{

    [SerializeField]
    private TMP_Text tutorialLabel;

    public bool tutorialEnabled;

    [HideInInspector]
    public int tutorialStage;
    private List<string> tutorialText;

    // Start is called before the first frame update
    void Start()
    {
        tutorialStage = 0;

        tutorialText = new List<string>();
        tutorialText.Add("Roll Button");
        tutorialText.Add("Inventory Button");
        tutorialText.Add("Shop Button");
        tutorialText.Add("Guide Button");
        tutorialText.Add("Tutorial Over");

        tutorialLabel.transform.parent.gameObject.SetActive(tutorialEnabled);
        tutorialLabel.text = tutorialText[tutorialStage];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextStage() {
        tutorialStage++;
        tutorialLabel.transform.parent.gameObject.SetActive(tutorialEnabled);
        tutorialLabel.text = tutorialText[tutorialStage];
    }
}

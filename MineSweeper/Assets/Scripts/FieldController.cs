using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FieldController : MonoBehaviour {
    public enum FIELD_STATE
    {
        OPEN_BOMB,
        MARKED_BOMB,
        UNDISCOVERED,
        OPEN_NUMBER,
        OPEN_EMPTY
    }
    public bool isBomb { get; set; }
    public bool isOpend { get; set; }
    public int xKoord { get; set; }
    public int yKoord { get; set; }
    public int surroundingBombs { get; set; }
    public FIELD_STATE state;

    private List<FieldController> neighbors = new List<FieldController>();

    [Header("Icons For Every Visible State")]
    public TextMeshProUGUI text;
    public Image bomb;
    public Image markedBomb;

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

	// Use this for initialization
	void Start () {
        state = FIELD_STATE.UNDISCOVERED;
	}
	
	// Update is called once per frame
	void Update () {
        switch (state)
        {
            case FIELD_STATE.OPEN_BOMB: displayBomb(); break;
            case FIELD_STATE.MARKED_BOMB: displayMarker(); break;
            case FIELD_STATE.UNDISCOVERED: displayDefault(); break;
            case FIELD_STATE.OPEN_NUMBER: displayNumber(); break;
            case FIELD_STATE.OPEN_EMPTY: displayEmpty(); break;
        }	
	}

    private void displayBomb()
    {
        bomb.enabled = true;
    }
    private void displayMarker()
    {
        markedBomb.enabled = true;
    }
    private void displayDefault()
    {
        bomb.enabled = false;
        markedBomb.enabled = false;
        text.enabled = false;
    }
    private void displayNumber()
    {
        text.enabled = true;
        text.text = surroundingBombs + "";
    }
    private void displayEmpty()
    {
        //ToDo!
    }
    public FIELD_STATE markField()
    {
        if (state == FIELD_STATE.UNDISCOVERED)
        {
            anim.Play("MarkField");
            state = FIELD_STATE.MARKED_BOMB;
        }
        else
        {          
            if (!isOpend)
            {
                anim.Play("MarkField_Reverse");
                state = FIELD_STATE.UNDISCOVERED;
            }
        }
        return state;
    }
    public bool openField()
    {
        if (isOpend || state == FIELD_STATE.MARKED_BOMB)
        {
            return false;
        }
        anim.Play("openField");
        isOpend = true;
        state = getState();
        GetComponent<Renderer>().material.color = Color.white;
        if(state == FIELD_STATE.OPEN_EMPTY)
        {
            foreach(FieldController neighbor in neighbors)
            {
                neighbor.openField();
            }
        }

        Debug.Log("Field: " + transform.position);
        return isBomb;        
    }
    private FIELD_STATE getState()
    {
        if (isBomb)
        {
            return FIELD_STATE.OPEN_BOMB;
        }
        if(surroundingBombs == 0)
        {
            return FIELD_STATE.OPEN_EMPTY;
        }
        else
        {
            return FIELD_STATE.OPEN_NUMBER;
        }
    }

    public List<FieldController> getNeighbors()
    {
        return neighbors;
    }
}

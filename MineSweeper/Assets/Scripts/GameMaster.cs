using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameMaster : MonoBehaviour {

    [Header("GameMode Settings")]
    public Transform player;
    public int CubeSize = 1;
    public int SPACE = 10;
    public int X_SIZE = 10;
    public int Y_SIZE = 10;
    [Range(0f,1f)]
    public float BOMB_RATE = 0.2f;

    [Header("UI Elements")]
    public TextMeshProUGUI bombs;
    public TextMeshProUGUI marked;
    public GameObject gameOverScreen;
    public GameObject victoryScreen;

    [Header("Settings")]
    public GameObject field;
    public Camera mainCam;

    //Singleton
    private static GameMaster gm;

    //Instancevariables
    public Dictionary<int, Dictionary<int, FieldController>> fields = new Dictionary<int, Dictionary<int, FieldController>>();
    private bool gameEnd;
    private bool won;
    private int bombsAmount;
    private int markedBombs;

    private enum FIELD_ACTION
    {
        OPEN,
        MARK
    }

    public static GameMaster GetInstance()
    {
        return gm;
    }

    void Awake()
    {
        gm = this;
    }

    void Start ()
    {
        int x, y;
        init(out x, out y);

        setCameraPosition();

        createCubes(ref x, ref y);

        setupCubeNeighbors(ref x, ref y);

        //Show marked bombs and amount of bombs
        marked.text = markedBombs + "";
        bombs.text = bombsAmount + "";
    }

    private void Update()
    {
        checkForGameOver();

        checkForInupt(FIELD_ACTION.OPEN);   //Open a field?

        checkForInupt(FIELD_ACTION.MARK);   //Mark a field?
    }


    /*
     * HILFSMETHODEN:
     */
    private void init(out int x, out int y)
    {
        gameEnd = false;
        won = false;
        x = 0;
        y = 0;
    }

    private void setCameraPosition()
    {
        //Position the Camera
        mainCam.transform.position = new Vector3(X_SIZE / 2 - 3, Y_SIZE / 2, mainCam.transform.position.z);
        mainCam.fieldOfView = (X_SIZE / 2 * Y_SIZE / 2);
    }

    private void createCubes(ref int x, ref int y)
    {
        //Spawn Field Objects
        for (int i = 0; i < X_SIZE * Y_SIZE; i++)
        {
            createNewCube(ref x, ref y);
        }
    }

    private void createNewCube(ref int x, ref int y)
    {
        GameObject cube = createCube(x, y);
        FieldController control = cube.GetComponent<FieldController>();
        control.xKoord = x;
        control.yKoord = y;

        addCubeToDictionary(x, y, control);

        x++;
        placeBombWithProbabilty(control);

        //New Row?
        if (x == X_SIZE)
        {
            x = 0;
            y++;
        }
    }

    private void placeBombWithProbabilty(FieldController control)
    {
        if (Random.value < BOMB_RATE)
        {
            control.isBomb = true;
            bombsAmount++;
        }
        else
        {
            control.isBomb = false;
        }
    }

    private void addCubeToDictionary(int x, int y, FieldController control)
    {
        //Add Field to Dictionary
        if (fields.ContainsKey(x))
        {
            if (!fields[x].ContainsKey(y))
            {
                fields[x].Add(y, control);
            }
        }
        else
        {
            Dictionary<int, FieldController> dicField = new Dictionary<int, FieldController>();
            dicField.Add(y, control);
            fields.Add(x, dicField);
        }
    }

    private GameObject createCube(int x, int y)
    {
        Vector3 cube_position = new Vector3(x * CubeSize + SPACE, y * CubeSize + SPACE, 0);
        GameObject cube = Instantiate(field, cube_position, Quaternion.identity, transform);
        return cube;
    }

    private void setupCubeNeighbors(ref int x, ref int y)
    {
        foreach (Dictionary<int, FieldController> row in fields.Values)
        {
            foreach (FieldController field in row.Values)
            {
                x = field.xKoord;
                y = field.yKoord;
                addNeighbors(x, y, field);
            }

        }
    }

    private void addNeighbors(int x, int y, FieldController field)
    {
        List<FieldController> neighbors = field.getNeighbors();
        for (int i = -1; i < 2; i++)
        {
            //Border of the field check
            if (x + i < 0 || x + i >= X_SIZE)
            {
                continue;
            }
            for (int j = -1; j < 2; j++)
            {
                //Border of the field check & handle cube isnt neighbor of it-self
                if (y + j < 0 || y + j >= Y_SIZE || (i == 0 && j == 0))
                {
                    continue;
                }

                neighbors.Add(fields[x + i][y + j]);

                //update Bombcount
                if (fields[x + i][y + j].isBomb)
                {
                    field.surroundingBombs++;
                }
            }
        }
    }
    private void checkForGameOver()
    {
        if (gameEnd)
        {
            if (won)
            {
                victoryScreen.SetActive(true);
            }
            else
            {
                gameOverScreen.SetActive(true);
            }
        }
        if (markedBombs == bombsAmount)
        {
            int bombsFound = markedBombs;
            undiscovered: foreach(Dictionary<int, FieldController> row in fields.Values)
            {
                foreach(FieldController field in row.Values)
                {
                    if(field.state == FieldController.FIELD_STATE.UNDISCOVERED)
                    {
                        return;
                    }
                    if(field.state == FieldController.FIELD_STATE.MARKED_BOMB && field.isBomb)
                    {
                        bombsFound--;
                    }
                }
            }
            Debug.Log("Bombs: " + markedBombs + " -- Found: " + bombsFound );
            if(bombsFound == 0)
            {
                gameEnd = true;
                won = true;
            }
        }      
    }

    private void checkForInupt(FIELD_ACTION action)
    {
        if (!gameEnd && getMouseButtonOnAction(action))
        {          
            RaycastHit hit;
            if(calculateRaycastHit(action,out hit))
            {
                performActionOnField(hit, action);
            }
        }
    }
    private bool calculateRaycastHit(FIELD_ACTION action, out RaycastHit hit)
    {
        if (player == null)
        {
            return Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, 100.0f);
        }
        else
        {
            return Physics.Raycast(player.position, transform.forward, out hit, 100.0f);
        }
        return false;
    }

    private static bool getMouseButtonOnAction(FIELD_ACTION action)
    {
        switch (action)
        {
            case FIELD_ACTION.OPEN: return Input.GetMouseButtonDown(0); break;
            case FIELD_ACTION.MARK: return Input.GetMouseButtonDown(1); break;
        }
        return false;
    }

    private void performActionOnField(RaycastHit hit, FIELD_ACTION action)
    {
        if (hit.transform != null && hit.transform.GetComponent<FieldController>() != null)
        {
            switch (action)
            {
                case FIELD_ACTION.OPEN: openField(hit); break;
                case FIELD_ACTION.MARK: markField(hit); break;
            }

        }
    }

    private void openField(RaycastHit hit)
    {
        FieldController control = hit.transform.GetComponent<FieldController>();
        gameEnd = control.openField();
        if (gameEnd)
        {
            Debug.Log("GameEnds!");
        }
    }
    private void markField(RaycastHit hit)
    {
        FieldController control = hit.transform.GetComponent<FieldController>();
        FieldController.FIELD_STATE state = control.markField();
        if (FieldController.FIELD_STATE.MARKED_BOMB == state)
        {
            markedBombs++;
        }
        else if (FieldController.FIELD_STATE.UNDISCOVERED == state)
        {
            markedBombs--;
        }
        marked.text = markedBombs + "";
    }
}

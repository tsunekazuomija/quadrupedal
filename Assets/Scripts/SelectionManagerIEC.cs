using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManagerIEC : MonoBehaviour
{
    [SerializeField] private string _saveFileName = "robots_save_data.json";
    public GameObject robotPrefab;
    public Slider survivalRateSlider;
    public int populationSize = 25;
    public float div = 5;
    public float generationTime = 60.0f;
    public float survivalRate = 0.3f; // 新しい変数: 生存率（上位何%を保持するか）
    private List<GameObject> robots;
    private List<Camera> cameras;
    public Toggle togglePrefab;
    public Button buttonPrefab;
    private Vector3 cameraOffset = new Vector3(0, 5, -5);
    private float generationTimer = 0.0f;
    private int robotVersion = 0;
    private int generation = 0;
    public Canvas canvas;
    private List<Toggle> toggles;

    void Start()
    {
        robots = new List<GameObject>();
        cameras = new List<Camera>();
        toggles = new List<Toggle>();

        for (int i = 0; i < populationSize; i++)
        {
            GameObject robot = Instantiate(robotPrefab, new Vector3(50 * (i / div), 3, 50 * (i % div)),
                Quaternion.Euler(0, 90, 90));
            robots.Add(robot);
            robot.GetComponent<CustomAttributes>().isSelected = false;
            robot.name = "" + robotVersion;
            robot.GetComponent<DisplayName>().SetName();
            robotVersion++;

            GameObject cameraObject = new GameObject("RobotCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameras.Add(camera);
            camera.rect = new Rect(
                1.0f / div * (int)(i / div) * 0.8f, 1.0f / div * (i % div), 1.0f / div * 0.8f, 1.0f / div);
            cameraObject.transform.localPosition = new Vector3(0, 2, -5);
            cameraObject.transform.localRotation = Quaternion.Euler(10, 0, 0);
        }

        ResetToggleButtons();
        Button button = Instantiate(buttonPrefab, canvas.transform);
        button.transform.position = new Vector3(Screen.width * 0.9f, Screen.height * 0.5f, 0);
        button.onClick.AddListener(SelectButtonClicked);

        ApplyGene();

        survivalRate = survivalRateSlider.value;
    }

    void FixedUpdate()
    {
        generationTimer += Time.fixedDeltaTime;

        for (int i = 0; i < populationSize; i++)
        {
            if (cameras[i] && robots[i])
            {
                cameras[i].transform.position = robots[i].transform.position + cameraOffset;
                cameras[i].transform.LookAt(robots[i].transform);
            }
        }
    }

    void ResetToggleButtons()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].onValueChanged.RemoveAllListeners();
            Destroy(toggles[i].gameObject);
        }
        toggles.Clear();

        for (int i = 0; i < robots.Count; i++)
        {
            robots[i].transform.position = new Vector3(50 * (i / div), 3, 50 * (i % div));
            Toggle toggle = Instantiate(togglePrefab, canvas.transform);
            Camera camera;
            camera = cameras[i];
            string name = robots[i].name;
            toggle.onValueChanged.AddListener(isSelected => OnToggleChanged(name, isSelected));
            Text label = toggle.GetComponentInChildren<Text>();
            label.text = "Robot " + robots[i].name;
            toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                (camera.rect.x) * Screen.width - 0.5f * Screen.width * 0.8f,
                (camera.rect.y - 0.5f + 0.5f / div) * Screen.height);
            toggles.Add(toggle);
        }

    }

    void SelectButtonClicked()
    {
        ApplyGene();
        SelectAndReproduce();
        Save();
        ResetRobots();
        Load();
        ChangePopulationSize();
        ResetToggleButtons();
        generation++;
    }

    void OnToggleChanged(string robotName, bool isSelected)
    {
        foreach (var robot in robots)
        {
            if (robot && robot.name == robotName)
            {
                CustomAttributes attributes = robot.GetComponent<CustomAttributes>();
                if (attributes)
                {
                    attributes.isSelected = isSelected;
                }
                break;
            }
        }
    }

    void SelectAndReproduce()
    {
        robots.RemoveAll(item => item == null);
        robots.Sort((a, b) =>
        {
            CustomAttributes attributesA = a.GetComponent<CustomAttributes>();
            CustomAttributes attributesB = b.GetComponent<CustomAttributes>();
            return attributesA.isSelected.CompareTo(attributesB.isSelected);
        });


        // Calculate the number of robots that will survive and be replaced
        int survivalCount = (int)(robots.Count * survivalRate);
        int replacementCount = robots.Count - survivalCount;

        for (int i = 0; i < replacementCount; i++)
        {
            // Crossover between two selected parents from the surviving robots
            var parent1 = robots[Random.Range(0, survivalCount)]
                .GetComponent<JointController>()
                .gene;
            var parent2 = robots[Random.Range(0, survivalCount)]
                .GetComponent<JointController>()
                .gene;
            var childGene = Crossover(parent1, parent2);

            // Apply mutation with a certain probability
            Mutate(childGene);

            // Replace the genes of the robots to be replaced with the new child genes
            robots[i + survivalCount].GetComponent<JointController>().gene =
                childGene;

            robots[i + survivalCount].transform.localScale =
                new Vector3(childGene.bodySizes[0], childGene.bodySizes[1],
                    childGene.bodySizes[2]);

            // 名前を変更
            robots[i + survivalCount].name = "" + robotVersion;
            robots[i + survivalCount].GetComponent<DisplayName>().SetName();
            robotVersion++;
        }
    }

    // Display the best gene and distance of the current generation
    void DisplayBestGeneAndDistance()
    {
        var bestGene = robots[0].GetComponent<JointController>().gene;
        string geneString = "";
        foreach (var angle in bestGene.angles)
        {
            geneString += angle.ToString() + ", ";
        }
        foreach (var legSize in bestGene.legSizes)
        {
            geneString += legSize.ToString() + ", ";
        }
        foreach (var bodySize in bestGene.bodySizes)
        {
            geneString += bodySize.ToString() + ", ";
        }
        Debug.Log("Generation: " + generation + ",Best distance: " +
            (robots[0].transform.position).sqrMagnitude);
    }

    // Crossover function to mix genes of two parents
    Gene Crossover(Gene parent1, Gene parent2)
    {
        Gene child = new Gene(parent1.angles.Count, parent1.legSizes.Count);

        // Decide the crossover point for angles
        int crossoverPointAngles = Random.Range(0, parent1.angles.Count);
        for (int i = 0; i < parent1.angles.Count; i++)
        {
            child.angles[i] =
                i < crossoverPointAngles ? parent1.angles[i] : parent2.angles[i];
        }

        // Decide the crossover point for legSizes
        int crossoverPointLegSizes = Random.Range(0, parent1.legSizes.Count);
        for (int i = 0; i < parent1.legSizes.Count; i++)
        {
            child.legSizes[i] = i < crossoverPointLegSizes ? parent1.legSizes[i]
                : parent2.legSizes[i];
        }

        // Decide the crossover point for bodySizes
        int crossoverPointBodySizes = Random.Range(0, parent1.bodySizes.Count);
        for (int i = 0; i < parent1.bodySizes.Count; i++)
        {
            child.bodySizes[i] = i < crossoverPointBodySizes ? parent1.bodySizes[i]
                : parent2.bodySizes[i];
        }

        // child.bodysizesの積を8.0にする
        float volume = child.bodySizes[0] * child.bodySizes[1] * child.bodySizes[2];
        float ratio = Mathf.Pow(8.0f / volume, 1.0f / 3.0f);
        child.bodySizes[0] *= ratio;
        child.bodySizes[1] *= ratio;
        child.bodySizes[2] *= ratio;

        return child;
    }

    // Mutate function to introduce random changes
    void Mutate(Gene gene)
    {
        // Mutation logic for angles
        for (int i = 0; i < gene.angles.Count; i++)
        {
            if (Random.Range(0.0f, 1.0f) < 0.1f)
            {
                gene.angles[i] = Random.Range(-60.0f, 60.0f);
            }
        }

        // Mutation logic for legSizes
        for (int i = 0; i < gene.legSizes.Count; i++)
        {
            if (Random.Range(0.0f, 1.0f) < 0.1f)
            {
                gene.legSizes[i] = Random.Range(0.1f, 0.5f);
            }
        }

        // Ensure the volume of the body remains constant after mutation
        if (Random.Range(0.0f, 1.0f) < 0.1f)
        {
            float volume = gene.bodySizes[0] * gene.bodySizes[1] * gene.bodySizes[2];
            gene.bodySizes[0] = Random.Range(1.5f, 3.0f);
            gene.bodySizes[1] = Random.Range(1.5f, 3.0f);
            gene.bodySizes[2] = volume / (gene.bodySizes[0] * gene.bodySizes[1]);
        }
    }

    // Change the size of the robot
    void ChangeRobotSize()
    {
        // 足のサイズをそれぞれ遺伝子から設定
        foreach (var robot in robots)
        {
            for (int i = 0; i < robot.GetComponent<JointController>().legParts.Count;
                i = i + 2)
            {
                var legPartR = robot.GetComponent<JointController>().legParts[i];
                var legPartL = robot.GetComponent<JointController>().legParts[i + 1];
                var legSizeX =
                  robot.GetComponent<JointController>().gene.legSizes[3 * i];
                var legSizeY =
                  robot.GetComponent<JointController>().gene.legSizes[3 * i + 1];
                var legSizeZ =
                  robot.GetComponent<JointController>().gene.legSizes[3 * i + 2];

                // legSizeを適用
                legPartR.transform.localScale =
                  new Vector3(legSizeX, legSizeY, legSizeZ);

                legPartL.transform.localScale =
                  new Vector3(legSizeX, legSizeY, legSizeZ);
            }
        }

        // 体のサイズをそれぞれ遺伝子から設定
        foreach (var robot in robots)
        {
            var body = robot.GetComponent<JointController>().body;
            var bodySizeX = robot.GetComponent<JointController>().gene.bodySizes[0];
            var bodySizeY = robot.GetComponent<JointController>().gene.bodySizes[1];
            var bodySizeZ = robot.GetComponent<JointController>().gene.bodySizes[2];

            // bodySizeを適用
            body.transform.localScale = new Vector3(bodySizeX, bodySizeY, bodySizeZ);
        }
    }

    void ChangePopulationSize()
    {
        // populationSize が0以下の場合は何もしない
        if (populationSize <= 0)
        {
            return;
        }

        int currentPopulation = robots.Count;

        // ロボットの数が目標より少ない場合、追加する
        while (currentPopulation < populationSize)
        {
            AddRobot();
            currentPopulation++;
        }

        // ロボットの数が目標より多い場合、削除する
        while (currentPopulation > populationSize)
        {
            RemoveRobot(currentPopulation - 1);
            currentPopulation--;
        }
    }

    void AddRobot()
    {
        GameObject robot = Instantiate(robotPrefab, new Vector3(0, 0, 0),
            Quaternion.Euler(0, 90, 90));
        robots.Add(robot);
        robot.GetComponent<CustomAttributes>().isSelected = false;
        robot.name = "" + robotVersion;
        robot.GetComponent<DisplayName>().SetName();
        robotVersion++;
    }

    void RemoveRobot(int index)
    {
        if (index >= 0 && index < robots.Count)
        {
            GameObject robotToRemove = robots[index];
            robots.RemoveAt(index);
            Destroy(robotToRemove);
        }
    }

    void ResetRobots()
    {
        // シーン上のロボットをすべて削除
        foreach (var robot in robots)
        {
            Destroy(robot);
        }
    }

    // ロボットのサイズを遺伝子に適用する
    public void ApplyGene()
    {
        for (int i = 0; i < robots.Count; i++)
        {
            // 胴体のサイズを遺伝子に適用
            robots[i].GetComponent<JointController>().gene.bodySizes[0] =
              robots[i].transform.localScale.x;
            robots[i].GetComponent<JointController>().gene.bodySizes[1] =
              robots[i].transform.localScale.y;
            robots[i].GetComponent<JointController>().gene.bodySizes[2] =
              robots[i].transform.localScale.z;

            // 足のサイズをそれぞれ遺伝子に適用
            for (int j = 0;
                j < robots[i].GetComponent<JointController>().legParts.Count;
                j = j + 2)
            {
                var legPartR = robots[i].GetComponent<JointController>().legParts[j];
                var legPartL =
                  robots[i].GetComponent<JointController>().legParts[j + 1];
                robots[i].GetComponent<JointController>().gene.legSizes[3 * j] =
                  legPartR.transform.localScale.x;
                robots[i].GetComponent<JointController>().gene.legSizes[3 * j + 1] =
                  legPartR.transform.localScale.y;
                robots[i].GetComponent<JointController>().gene.legSizes[3 * j + 2] =
                  legPartR.transform.localScale.z;
            }
        }
    }

    public void SetSurvivalRate() { survivalRate = survivalRateSlider.value; }

    // Saveロジックの例
    public void Save()
    {
        List<GeneData> geneDataList = new List<GeneData>();
        foreach (var robot in robots)
        {
            Gene gene = robot.GetComponent<JointController>().gene;
            GeneData geneData = new GeneData(gene);
            geneData.angles = gene.angles;
            geneData.legSizes = gene.legSizes;
            geneData.bodySizes = gene.bodySizes;
            geneData.name = int.Parse(robot.name);
            geneDataList.Add(geneData);
        }
        SaveLoadManager.Instance.SaveRobotData(_saveFileName, geneDataList);
    }

    // Loadロジックの例
    public void Load()
    {
        GeneDataList geneDataList = SaveLoadManager.Instance.LoadRobotData(_saveFileName);
        if (geneDataList != null)
        {
            robots = new List<GameObject>();
            foreach (var geneData in geneDataList.geneDatas)
            {
                GameObject robot = Instantiate(robotPrefab, new Vector3(0, 3, 0),
                    Quaternion.Euler(0, 90, 90));
                //  public Gene(int numAngles, int numLegSizes)
                robot.GetComponent<JointController>().gene =
                  new Gene(geneData.angles.Count, geneData.legSizes.Count);
                for (int i = 0; i < geneData.angles.Count; i++)
                {
                    robot.GetComponent<JointController>().gene.angles[i] =
                      geneData.angles[i];
                }
                robot.GetComponent<JointController>().gene.legSizes = geneData.legSizes;
                robot.GetComponent<JointController>().gene.bodySizes =
                  geneData.bodySizes;
                robot.name = geneData.name.ToString();
                // robotVersionとgeneData.nameの大きい方をrobotVersionにする
                robotVersion = Mathf.Max(robotVersion, geneData.name);
                robots.Add(robot);
                robot.GetComponent<DisplayName>().SetName();
            }

            ChangeRobotSize();
        }
    }
}

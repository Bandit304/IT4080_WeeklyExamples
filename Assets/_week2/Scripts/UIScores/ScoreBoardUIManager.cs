// using System;
// using System.Collections.Generic;
// using TreeEditor;
// using Unity.Entities;
// using Unity.NetCode;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;

// public class ScoreBoardUIManager : MonoBehaviour
// {
    
//     public UIDocument uiDocument;
//     private MultiColumnListView scoreBoard;
//     private List<PlayerScore> playerScores = new List<PlayerScore>();

//     private EntityManager entityManager;
//     private EntityQuery ghostQuery;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         var root = uiDocument.rootVisualElement;
//         scoreBoard = root.Q<MultiColumnListView>("ScoreboardMultiColListView");
//         scoreBoard.columns.Add(new Column()
//         {
//             title = "NAME",
//             makeCell = MakeCellLabel,
//             bindCell = BindNameToCell,
//             stretchable = true,
//         });
//         BuildMockUpDisplay();
//         scoreBoard.Rebuild();
//     }
//     private void BuildMockUpDisplay()
//     {
//         var persons = new List<PlayerScore>()
//             {
//                 new PlayerScore("John", 20,2),
//                 new PlayerScore("Jane", 23,2),
//             };
//         ApplyPersons(persons);
//     }
//     public void ApplyPersons(List<PlayerScore> persons)
//     {
//         scoreBoard.itemsSource = persons;
//     }
//     // Update is called once per frame
//     void Update()
//     {
      
//     }

//     private Label MakeCellLabel() => new();

//     private void BindNameToCell(VisualElement element, int index)
//     {
//         var label = (Label)element;
//         var person = (PlayerScore)scoreBoard.viewController.GetItemForIndex(index);
//         label.text = person.playerName;
//     }

//     private void BindDeathsToCell(VisualElement element, int index)
//     {
//         var label = (Label)element;
//         var playerScore = (PlayerScore)scoreBoard.viewController.GetItemForIndex(index);
//         label.text = ""+playerScore.kills;
//     }
// }
// public class PlayerScore
// {
//     public string playerName;
//     public int kills;
//     public int deaths;

//     public PlayerScore(string pName, int kill, int death)
//     {
//         playerName = pName;
//         kills = kill;
//         deaths = death;
//     }
// }

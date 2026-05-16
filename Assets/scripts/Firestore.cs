using Firebase.Extensions;
using Firebase.Firestore; // se encuentra dentro del package de firebase, se tiene que instalar el package para usarlo

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Firestore : MonoBehaviour
{
    FirebaseFirestore db;
    async void Start()
    {
        db = FirebaseFirestore.DefaultInstance; // obtenemos la instancia de la base de datos esta seria la default (inicializacion de fire base )
        await AddDataToCollection();
        await ReadAllData();
    }

    public async Task AddDataToCollection()
    {
        // comunicacion asincrona, se hace de esta manera para no bloquear el hilo principal, es decir, el juego no se va a congelar mientras se hace la comunicacion con firebase
        DocumentReference docRef = db.Collection("users").Document("memo"); // db/users/memo referencia a donde se esta creando 
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "Name", "Memo" },
            { "Lastname", "Reyes" },
            { "Born", 2000 }
        };

        await docRef.SetAsync(user);

        Debug.Log("Added Document to firebase");
    }

    public async Task ReadAllData()
    {
        CollectionReference usersRef = db.Collection("users"); // db/collection
        QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            //Referencia a mi tabla = table
            // var row = Instantiate(prefabRow, vector.zero, table);
            // row.GetComponent<RowController>().SetRowContent(document.name, document.highscore);
        }

        Debug.Log("Readed all info");
    }

    public async Task ReadOneDocument()
    {
        DocumentReference usersRef = db.Collection("users").Document("memo");
        DocumentSnapshot snapshot = await usersRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            Debug.Log($"User: {snapshot.Id}");

            Dictionary<string, object> userData = snapshot.ToDictionary();

            if (userData.TryGetValue("Name", out var name))
                Debug.Log($"Name: {name}");

            if (userData.TryGetValue("Lastname", out var lastname))
                Debug.Log($"Lastname: {lastname}");

            if (userData.TryGetValue("Born", out var born))
                Debug.Log($"Year: {born}");
        }

        Debug.Log("Readed one document info");
    }
}
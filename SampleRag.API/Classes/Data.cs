using Microsoft.Extensions.VectorData;

namespace SampleRagAPI.RagAPI.Classes;

internal class Data
{
    private static List<Pokemon> myRecords;

    public Data()
    {
        myRecords = new List<Pokemon>();
        CreateRecords();
    }

    private void CreateRecords()
    {
        myRecords = new List<Pokemon>
        {
            new Pokemon { Id = 1, Name = "Bulbasaur", Description = "A Grass/Poison type Pokémon." },
            new Pokemon { Id = 2, Name = "Charmander", Description = "A Fire type Pokémon." },
            new Pokemon { Id = 3, Name = "Squirtle", Description = "A Water type Pokémon." },
            // Add more Pokémon as needed
        };
    }

    public List<Pokemon> RetrieveRecords()
    {
        return myRecords;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ArrayWrapper
{
    public int rows;
    public int cols;
    public int[] data;
}

public class ArraySerializer : MonoBehaviour
{
    public static string SaveArray(int[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        
        int[] flatArray = FlattenArray(array);
        
        ArrayWrapper wrapper = new ArrayWrapper
        {
            rows = rows,
            cols = cols,
            data = flatArray
        };
        
        return JsonUtility.ToJson(wrapper, true);
    }

    public static int[,] LoadArray(string jsonString)
    {
        ArrayWrapper wrapper = JsonUtility.FromJson<ArrayWrapper>(jsonString);
        
        return UnflattenArray(wrapper.data, wrapper.rows, wrapper.cols);
    }

    private static int[] FlattenArray(int[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        
        int[] flatArray = new int[rows * cols];
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                flatArray[i * cols + j] = array[i, j];
            }
        }
        
        return flatArray;
    }

    private static int[,] UnflattenArray(int[] flatArray, int rows, int cols)
    {
        int[,] unflattenedArray = new int[rows, cols];
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                unflattenedArray[i, j] = flatArray[i * cols + j];
            }
        }
        
        return unflattenedArray;
    }
}

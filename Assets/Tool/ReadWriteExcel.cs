using System.IO;
using UnityEngine;
using OfficeOpenXml;
using UnityEditor;
using System.Text.RegularExpressions;
using TMPro;

public class ReadWriteExcel : MonoBehaviour
{
    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        string fileName = "D:/Report/大物实验/铁磁材料/物理实验十七.xlsx";
        FileInfo fileInfo = new FileInfo(fileName);

        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            double N = 50, n = 150, L = 60, S = 80;
            double R1 = 2.5, R2 = 30;
            double C2 = 6;

            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

            for (int i = 2; i <= 61; ++i)
            {
                double Uh = (double)worksheet.Cells[i, 1].Value;
                double Ub = (double)worksheet.Cells[i, 2].Value;
                Debug.Log("Uh:" + Uh + " " + "Ub:" + Ub);
                double H = N * Uh / (L * R1);
                double B = C2 * R2 * Ub / (n * S);
                worksheet.Cells[i, 3].Value = B;
                worksheet.Cells[i, 4].Value = H;
                worksheet.Cells[i, 5].Value = B / H;
                Debug.Log("B:" + B + " " + "H:" + H);
                Debug.Log(worksheet.Cells[i, 3].Value + " " + worksheet.Cells[i, 4].Value);
            }

            package.Save();
        }
    }

}

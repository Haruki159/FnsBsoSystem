using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnsBsoSystem.Class
{
    public class WriteOffActDto
    {
        // Данные о сотруднике (из Main_Employees, Ref_Departments, Ref_Positions)
        public string EmployeeFullName { get; set; } // FullName
        public string DepartmentName { get; set; }   // DeptName
        public string PositionName { get; set; }     // PosName

        // Данные о бланках (из Main_Inventory и Ref_BlankTypes)
        public string BlankTypeName { get; set; }    // TypeName
        public string Series { get; set; }           // Series
        public int StartNumber { get; set; }         // StartNumber
        public int EndNumber { get; set; }           // EndNumber

        // Данные с формы (вводит пользователь при списании)
        public string Reason { get; set; }           // Причина списания
    }
}

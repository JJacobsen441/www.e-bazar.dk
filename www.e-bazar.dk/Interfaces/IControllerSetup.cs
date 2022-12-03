using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Interfaces
{
    interface IControllerSetup
    {
        dto_person current_user { get; set; }
        Access access { get; set; }        
    }
}

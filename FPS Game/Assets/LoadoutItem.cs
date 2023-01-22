using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutItem : MonoBehaviour
{
    public Gun gun;

    public void OnClickSwitchWeapon()
    {
        transform.root.GetComponent<PauseMenu>().ChangeWeapon(gun.index);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutItem : MonoBehaviour
{
    public SingleShotGun gun;

    public void OnClickSwitchWeapon()
    {
        transform.root.GetComponent<PauseMenu>().ChangeWeapon(gun.index);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickerState
{
    IPickerState DoState(PickerLogic picker);
}

public class IdleState : IPickerState
{
    public IPickerState DoState(PickerLogic picker)
    {
        if (BlockInstantiator.instance.BlockList.Count == 0)
        {
            return picker.IdleState;
        }
        else
        {
            return picker.SetTargetState;
        }
    }
}

public class MovingToState : IPickerState
{
    public IPickerState DoState(PickerLogic picker)
    {
        var distanceX = picker.target.position.x - picker.transform.position.x;

        if (Mathf.Abs(distanceX) <= 0.08f)
        {
            picker.movement = Vector2.zero;

            if (picker.target == picker.middlePos)
            {
                return picker.IdleState;
            }
            else if (picker.blocksInRange.Contains(picker.target.gameObject))
            {
                return picker.PickUpState;
            }
            else if (picker.target.tag == "Deposit" && picker.carryingBlock != null)
            {
                return picker.DepositState;
            }
        }
        else if (picker.carryingBlock == null && picker.lastBlockListCount != BlockInstantiator.instance.BlockList.Count)
        {
            picker.lastBlockListCount = BlockInstantiator.instance.BlockList.Count;
            return picker.SetTargetState;
        }

        picker.movement = new Vector2(distanceX, 0f).normalized;
        return picker.MovingToState;
    }
}

public class SetTargetState : IPickerState
{
    public IPickerState DoState(PickerLogic picker)
    {
        if (picker.carryingBlock == null)
        {
            if (BlockInstantiator.instance.BlockList.Count > 0)
            {
                GameObject closestBlock = null;
                var minDistance = float.MaxValue;
                foreach (var block in BlockInstantiator.instance.BlockList)
                {
                    var tempDistance = Mathf.Abs(picker.transform.position.x - block.transform.position.x);
                    if (tempDistance < minDistance)
                    {
                        minDistance = tempDistance;
                        closestBlock = block;
                    }
                }

                picker.target = closestBlock.transform;
            }
            else
            {
                picker.target = picker.middlePos;
            }
        }
        else
        {
            var blockColor = picker.carryingBlock.GetComponent<Block>().color;
            picker.target = (blockColor == BlockColour.Blue) ? picker.depositBlue : picker.depositRed;
        }

        return picker.MovingToState;
    }
}

public class PickUpState : IPickerState
{
    public IPickerState DoState(PickerLogic picker)
    {
        //Debug.Log("Picking up");
        var objectToPickUp = picker.blocksInRange.Find(x => x == picker.target.gameObject);

        picker.blocksInRange.Remove(objectToPickUp);
        objectToPickUp.SetActive(false);
        picker.carryingBlock = objectToPickUp;

        return picker.SetTargetState;
    }
}

public class DepositState : IPickerState
{
    public IPickerState DoState(PickerLogic picker)
    {
        var targetColour = picker.target.GetComponent<Block>().color;
        if (targetColour == picker.carryingBlock.GetComponent<Block>().color)
        {
            BlockInstantiator.instance.InsertBlockIntoDeposit(picker.carryingBlock);
            picker.carryingBlock = null;
        }

        return picker.SetTargetState;
    }
}


using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yates.Runtime.Promise;

public class PromiseTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var buttons = this.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(Test1);
        buttons[1].onClick.AddListener(Test2);
        buttons[2].onClick.AddListener(Test3);
        buttons[3].onClick.AddListener(Test4);
    }

    /// <summary>
    /// 多个同步且无返回值的函数序列
    /// </summary>
    private void Test1()
    {
        var pro = Promise.Create(((resolve, reject) =>
        {
            Debug.LogError("1");
            resolve();
        }));

        pro.Then(() =>
        {
            Debug.LogError("2");
        }).Then(() =>
        {
            Debug.LogError("3");
        }).Then((() =>
        {
            Debug.LogError("4");
        }));
    }

    /// <summary>
    /// 多个异步操作序列
    /// </summary>
    private void Test2()
    {
        var pro = Promise.Create(((resolve, reject) =>
        {
            Debug.LogError("1");
            resolve();
        }));

        pro.Then(() =>
        {
            Debug.LogError("2");
            return this.Delay(3);
        }).Then(() =>
        {
            Debug.LogError("3");
            return this.Delay(1);
        }).Then((() =>
        {
            Debug.LogError("4");
        }));
    }

    /// <summary>
    /// 多个同步有参数和无参数混用序列
    /// </summary>
    private void Test3()
    {
        var pro = Promise.Create(((resolve, reject) =>
        {
            Debug.LogError("1");
            resolve();
        }));

        pro.Then(() =>
        {
            Debug.LogError("2");
        }).Then(() =>
        {
            Debug.LogError("3");
            return Promise<int>.Create(((resolve, _) =>
            {
                resolve(4);
            } ));
        }).Then(((result) =>
        {
            Debug.LogError(result);
            Debug.LogError("5");
        }));
    }

    /// <summary>
    /// 多个异步同步有参数和无参数混用序列
    /// </summary>
    private void Test4()
    {
        var pro = Promise.Create(((resolve, reject) =>
        {
            Debug.LogError("1");
            resolve();
        }));

        pro.Then(() =>
        {
            Debug.LogError("2");
        }).Then(() =>
        {
            Debug.LogError("3");
            return Promise<int>.Create(((resolve, _) => { StartCoroutine(DelayIE(3, (() => { resolve(4); }))); }));
        }).Then(((result) =>
        {
            Debug.LogError(result);
        }));
    }

    private Promise Delay(float time)
    {
        return Promise.Create(((resolve, _) => { StartCoroutine(DelayIE(time, resolve)); }));
    }

    private IEnumerator DelayIE(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }
}

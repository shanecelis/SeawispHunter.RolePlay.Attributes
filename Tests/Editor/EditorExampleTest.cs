using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SeawispHunter.RolePlay.Attributes;

namespace SeawispHunter.RolePlay.Attributes.Tests {
  public class EditorExampleTest {
    // A Test behaves as an ordinary method
    //
    [Test] public void TestModifierToString0() {
      var modifier = Modifier.FromFunc((float x) => x + 1);
      Assert.AreEqual("?f()", modifier.ToString());

    }

    [Test] public void TestModifierToString1() {
      var modifier = Modifier.FromFunc((float x) => x + 1, "x + 1");
      Assert.AreEqual("x + 1", modifier.ToString());

    }
  }
}

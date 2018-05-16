// ***********************************************************
//	Copyright 2016 Next Limit Technologies, http://www.nextlimit.com
//	All rights reserved.
//
//	THIS SOFTWARE IS PROVIDED 'AS IS' AND WITHOUT ANY EXPRESS OR
//	IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// ***********************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  /// <summary>
  /// A node can hold one or more CNField, holds references to scene gameobjects,
  /// name selector and other nodes.
  /// </summary>
  [System.Serializable]
  public class CNField : IDeepClonable<CNField>
  {
    [Flags]
    public enum ScopeFlag
    {
      Global,
      Inherited, 
    }

    [SerializeField]
    private bool exclusive_;
    public bool IsExclusive
    {
      get
      {
        return exclusive_;
      }
    }
    
    [SerializeField]
    List<CommandNode> lCommandNodes_ = new List<CommandNode>();
    public List<CommandNode> CommandNodes
    {
      get 
      {
        if (lCommandNodes_ == null)
        {
          lCommandNodes_ = new List<CommandNode>();
        }
        return lCommandNodes_;
      }
    }

    public int NumberOfNodes
    {
      get { return (CommandNodes.Count ); }
    }

    [SerializeField]
    List<string> lNameSelector_ = new List<string>();
    public List<string> NameSelectors
    {
      get
      {
        if (lNameSelector_ == null)
        {
          lNameSelector_ = new List<string>();
        }
        return lNameSelector_;
      }
    }

    [SerializeField]
    List<GameObject> lGameObject_ = new List<GameObject>();
    public List<GameObject> GameObjects
    {
      get
      {
        if (lGameObject_ == null)
        {
          lGameObject_ = new List<GameObject>();
        }
        return lGameObject_;
      }
    }

    [SerializeField]
    private ScopeFlag scopeType_;
    public ScopeFlag ScopeType
    {
      get { return scopeType_; }
      set { scopeType_ = value; }
    }

    [SerializeField]
    CNFieldContentType contentType_;
    public CNFieldContentType ContentType
    {
      get { return contentType_; }
      set { contentType_ = value; }
    }

    [SerializeField]
    bool showOwnerGroupOnly_ = false;
    public bool ShowOwnerGroupOnly
    {
      get { return showOwnerGroupOnly_; }
      set { showOwnerGroupOnly_ = value; }
    }
    //-----------------------------------------------------------------------------------
    public CNField( bool exclusive, CNFieldContentType allowedType, ScopeFlag scopeType, bool addDefaultWildcard )
    { 
      contentType_  = allowedType;
      exclusive_    = exclusive;
      ScopeType     = scopeType;

      if (addDefaultWildcard)
      {
        NameSelectors.Add("*");
      }
    }
    //-----------------------------------------------------------------------------------
    public CNField( bool exclusive, CNFieldContentType allowedType, bool addDefaultWildcard )
      : this( exclusive, allowedType, ScopeFlag.Inherited, addDefaultWildcard)
    {

    }
    //-----------------------------------------------------------------------------------
    public CNField(bool exclusive, bool addDefaultWildcard )
      : this(exclusive, CNFieldContentType.Geometry, ScopeFlag.Inherited, addDefaultWildcard)
    {

    }
    //-----------------------------------------------------------------------------------
    private CNField(CNField original)
    {
      exclusive_   = original.exclusive_;
      contentType_ = original.contentType_;
      scopeType_   = original.scopeType_;
      
      //references to gameObjects are never cloned 

      foreach (CommandNode commandNode in original.lCommandNodes_)
      {
        CommandNodes.Add( commandNode );
      }

      foreach (string nameSelector in original.lNameSelector_)
      {
        NameSelectors.Add( string.Copy( nameSelector ) );
      }
    }
    //-----------------------------------------------------------------------------------
    public bool AreGameObjectsAllowed()
    {
      return ( contentType_.IsFlagSet(CNFieldContentType.GameObject)  );
    }
    //-----------------------------------------------------------------------------------
    public void Clear()
    {
      CommandNodes.Clear();
      NameSelectors.Clear();
      GameObjects.Clear();
    }
    //-----------------------------------------------------------------------------------
    public bool ContainsNode(CommandNode node)
    {
      return lCommandNodes_.Contains(node);
    }
    //-----------------------------------------------------------------------------------
    public bool AddNode( CommandNode node )
    {
      if (!lCommandNodes_.Contains(node))
      {
        lCommandNodes_.Add(node);
        return true;
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool RemoveNode( CommandNode node )
    {
      return (lCommandNodes_.Remove(node));
    }
    //-----------------------------------------------------------------------------------
    public void RemoveNodeAt( int nodeIdx )
    {
      lCommandNodes_.RemoveAt(nodeIdx);
    }
    //-----------------------------------------------------------------------------------
    public void RemoveNodesAt( List<uint> listNodeIdxToRemove )
    {
      List<CommandNode> listNode = new List<CommandNode>();
      foreach( uint nodeIdx in listNodeIdxToRemove )
      {
        listNode.Add( lCommandNodes_[(int)nodeIdx] );
      }
      foreach( CommandNode nodeToDelete in listNode )
      {
        lCommandNodes_.Remove( nodeToDelete );
      }
    }
    //-----------------------------------------------------------------------------------
    public void PurgeInvalidNullNodes()
    {
      if ( lCommandNodes_ != null )
      {
        int lastNode = lCommandNodes_.Count - 1;

        for (int i = lastNode; i >= 0; i--)
        {
          if (lCommandNodes_[i] == null )
          {
            lCommandNodes_.RemoveAt(i);
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public CommandNode GetNode(int nodeIdx)
    {
      return lCommandNodes_[nodeIdx];
    }
    //-----------------------------------------------------------------------------------
    public string GetNodeName( int nodeIdx )
    {
      return lCommandNodes_[nodeIdx].Name;
    }
    //-----------------------------------------------------------------------------------
    public string GetNodeListName(int nodeIdx)
    {
      return lCommandNodes_[nodeIdx].ListName;
    }
    //-----------------------------------------------------------------------------------
    public void GetFieldLists(ref List<CommandNode> lCommandNode, ref List<string> lNameSelector, ref List<GameObject> lGameObject)
    {
      lCommandNode        = lCommandNodes_;
      lNameSelector       = lNameSelector_;
      lGameObject         = lGameObject_;
    }
    //-----------------------------------------------------------------------------------
    public CNField DeepClone()
    {
      CNField clonedField = new CNField(this);
      return clonedField;
    }
    //-----------------------------------------------------------------------------------
    public bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool wasAnyUpdate = false;
      int nCommandNode = lCommandNodes_.Count;

      for (int i = 0; i < nCommandNode; i++)
      {
        CommandNode node = lCommandNodes_[i];
        if ( dictNodeToClonedNode.ContainsKey(node) )
        {
          lCommandNodes_[i] = dictNodeToClonedNode[node];
          wasAnyUpdate = true;
        }
      }
      return wasAnyUpdate;
    }

  } // class CNField

} //namespace CaronteFX...

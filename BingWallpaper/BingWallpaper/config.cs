﻿using System;
using System.Xml;
using System.Windows.Forms;
using System.IO;

///<summary>
/// ConfigXML XML文档操作
///</summary>
public static class ConfigXML
{
    #region XML文档节点查询和读取
    ///<summary>
    /// 选择匹配XPath表达式的第一个节点XmlNode.
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名")</param>
    ///<returns>返回XmlNode</returns>
    public static XmlNode GetXmlNodeByXpath(string xmlFileName, string xpath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            return xmlNode;
        }
        catch 
        {
            MessageBox.Show("配置文件损坏，自动重建！");
            File.Delete(imageInfo.XMLPath);
            ConfigXML.CreateXmlDocument(imageInfo.XMLPath, "config", "1.0", "UTF-8", "yes");
            ConfigXML.CreateOrUpdateXmlNodeByXPath(imageInfo.XMLPath, @"config/general", "AppVersion", "1.0.0.20170422");
            ConfigXML.CreateOrUpdateXmlNodeByXPath(imageInfo.XMLPath, @"config/general", "delayMinute", "30");
            return null;
   
        }
    }

    ///<summary>
    /// 读取延时时间.
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名")</param>
    ///<param name="defVal">节点不存在返回的默认时间</param>
    ///<returns>返回默认时间</returns>
    public static string GetDelayTime(string xmlFileName, string xpath, string defVal)
    {
        XmlNode node = GetXmlNodeByXpath(xmlFileName, xpath);
        if (node == null)   //节点不存在
            return defVal;
        try
        {
            int time = Convert.ToInt32(node.InnerText);
            if (time >= 0 && time <= 1200)   //不超过20个小时
                return node.InnerText;
            else
                return defVal;
        }
        catch
        {
            return defVal;
        }
        
    }

    ///<summary>
    /// 选择匹配XPath表达式的第一个节点文本.
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名")</param>
    ///<returns>返回该节点的文本值</returns>
    public static string GetXmlNodeInnerTextByXpath(string xmlFileName, string xpath)
    {
        XmlNode node = GetXmlNodeByXpath(xmlFileName, xpath);
        if (node == null)   //节点不存在
            return string.Empty;
        return node.InnerText;

    }

    ///<summary>
    /// 选择匹配XPath表达式的节点列表XmlNodeList.
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名")</param>
    ///<returns>返回XmlNodeList</returns>
    public static XmlNodeList GetXmlNodeListByXpath(string xmlFileName, string xpath)
    {
        XmlDocument xmlDoc = new XmlDocument();

        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(xpath);
            return xmlNodeList;
        }
        catch //(Exception ex)
        {
            return null;
            //throw ex;
        }
    }

    ///<summary>
    /// 选择匹配XPath表达式的第一个节点的匹配xmlAttributeName的属性XmlAttribute.
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名</param>
    ///<param name="xmlAttributeName">要匹配xmlAttributeName的属性名称</param>
    ///<returns>返回xmlAttributeName</returns>
    public static XmlAttribute GetXmlAttribute(string xmlFileName, string xpath, string xmlAttributeName)
    {
        string content = string.Empty;
        XmlDocument xmlDoc = new XmlDocument();
        XmlAttribute xmlAttribute = null;
        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            if (xmlNode != null)
            {
                if (xmlNode.Attributes.Count > 0)
                {
                    xmlAttribute = xmlNode.Attributes[xmlAttributeName];
                }
            }
        }
        catch //(Exception ex)
        {
          //  throw ex;
        }
        return xmlAttribute;
    }
    #endregion

    #region XML文档创建和节点或属性的添加、修改
    ///<summary>
    /// 创建一个XML文档
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="rootNodeName">XML文档根节点名称(须指定一个根节点名称)</param>
    ///<param name="version">XML文档版本号(必须为:"1.0")</param>
    ///<param name="encoding">XML文档编码方式</param>
    ///<param name="standalone">该值必须是"yes"或"no",如果为null,Save方法不在XML声明上写出独立属性</param>
    ///<returns>成功返回true,失败返回false</returns>
    public static bool CreateXmlDocument(string xmlFileName, string rootNodeName, string version, string encoding, string standalone)
    {
        bool isSuccess = false;
        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration(version, encoding, standalone); //创建类型声明节点  
            xmlDoc.AppendChild(xmlDeclaration);

            XmlNode root = xmlDoc.CreateElement(rootNodeName); //创建根节点 
            xmlDoc.AppendChild(root);

            XmlElement body = xmlDoc.CreateElement("general");  //创建子节点1
            root.AppendChild(body);
            body = xmlDoc.CreateElement("version");  //创建子节点2
            root.AppendChild(body);

          //  XmlNode rootNode = xmlDoc.SelectSingleNode(rootNodeName);
            // body = xmlDoc.CreateElement("soft");
            //body.SetAttribute("name", "dd");
            //rootNode.AppendChild(body);
              

            xmlDoc.Save(xmlFileName);
            isSuccess = true;
        }
        catch 
        {
           MessageBox.Show("创建XML文件出错");
        }
        return isSuccess;
    }

    ///<summary>
    /// 依据匹配XPath表达式的第一个节点来创建它的子节点(如果此节点已存在则追加一个新的同名节点
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名</param>
    ///<param name="xmlNodeName">要匹配xmlNodeName的节点名称</param>
    ///<param name="innerText">节点文本值</param>
    ///<param name="xmlAttributeName">要匹配xmlAttributeName的属性名称</param>
    ///<param name="value">属性值</param>
    ///<returns>成功返回true,失败返回false</returns>
    public static bool CreateXmlNodeByXPath(string xmlFileName, string xpath, string xmlNodeName, string innerText, string xmlAttributeName, string value)
    {
        bool isSuccess = false;
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            if (xmlNode != null)
            {
                //存不存在此节点都创建
                XmlElement subElement = xmlDoc.CreateElement(xmlNodeName);
                subElement.InnerXml = innerText;

                //如果属性和值参数都不为空则在此新节点上新增属性
                if (!string.IsNullOrEmpty(xmlAttributeName) && !string.IsNullOrEmpty(value))
                {
                    XmlAttribute xmlAttribute = xmlDoc.CreateAttribute(xmlAttributeName);
                    xmlAttribute.Value = value;
                    subElement.Attributes.Append(xmlAttribute);
                }

                xmlNode.AppendChild(subElement);
            }
            xmlDoc.Save(xmlFileName); //保存到XML文档
            isSuccess = true;
        }
        catch
        {
            //throw ex; 
        }
        return isSuccess;
    }

    ///<summary>
    /// 依据匹配XPath表达式的第一个节点来创建或更新它的子节点(如果节点存在则更新,不存在则创建)
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名</param>
    ///<param name="xmlNodeName">要匹配xmlNodeName的节点名称</param>
    ///<param name="innerText">节点文本值</param>
    ///<returns>成功返回true,失败返回false</returns>
    public static bool CreateOrUpdateXmlNodeByXPath(string xmlFileName, string xpath, string xmlNodeName, string innerText)
    {
        bool isSuccess = false;
        bool isExistsNode = false;//标识节点是否存在
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            if (xmlNode != null)
            {
                //遍历xpath节点下的所有子节点
                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    if (node.Name.ToLower() == xmlNodeName.ToLower())
                    {
                        //存在此节点则更新
                        node.InnerXml = innerText;
                        isExistsNode = true;
                        break;
                    }
                }
                if (!isExistsNode)
                {
                    //不存在此节点则创建
                    XmlElement subElement = xmlDoc.CreateElement(xmlNodeName);
                    subElement.InnerXml = innerText;
                    xmlNode.AppendChild(subElement);
                }
            }
            xmlDoc.Save(xmlFileName); //保存到XML文档
            isSuccess = true;
        }
        catch 
        {
            //throw ex; 
        }
        return isSuccess;
    }

    ///<summary>
    /// 依据匹配XPath表达式的第一个节点来创建或更新它的属性(如果属性存在则更新,不存在则创建)
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名</param>
    ///<param name="xmlAttributeName">要匹配xmlAttributeName的属性名称</param>
    ///<param name="value">属性值</param>
    ///<returns>成功返回true,失败返回false</returns>
    public static bool CreateOrUpdateXmlAttributeByXPath(string xmlFileName, string xpath, string xmlAttributeName, string value)
    {
        bool isSuccess = false;
        bool isExistsAttribute = false;//标识属性是否存在
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            if (xmlNode != null)
            {
                //遍历xpath节点中的所有属性
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    if (attribute.Name.ToLower() == xmlAttributeName.ToLower())
                    {
                        //节点中存在此属性则更新
                        attribute.Value = value;
                        isExistsAttribute = true;
                        break;
                    }
                }
                if (!isExistsAttribute)
                {
                    //节点中不存在此属性则创建
                    XmlAttribute xmlAttribute = xmlDoc.CreateAttribute(xmlAttributeName);
                    xmlAttribute.Value = value;
                    xmlNode.Attributes.Append(xmlAttribute);
                }
            }
            xmlDoc.Save(xmlFileName); //保存到XML文档
            isSuccess = true;
        }
        catch 
        {
          //  throw ex; 
        }
        return isSuccess;
    }
    #endregion


    #region XML文档节点或属性的删除
    ///<summary>
    /// 删除匹配XPath表达式的第一个节点(节点中的子元素同时会被删除)
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名</param>
    ///<returns>成功返回true,失败返回false</returns>
    public static bool DeleteXmlNodeByXPath(string xmlFileName, string xpath)
    {
        bool isSuccess = false;
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            if (xmlNode != null)
            {
                //删除节点
                xmlNode.ParentNode.RemoveChild(xmlNode);
            }
            xmlDoc.Save(xmlFileName); //保存到XML文档
            isSuccess = true;
        }
        catch
        {
           // throw ex;
        }
        return isSuccess;
    }

    ///<summary>
    /// 删除匹配XPath表达式的第一个节点中的匹配参数xmlAttributeName的属性
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名</param>
    ///<param name="xmlAttributeName">要删除的xmlAttributeName的属性名称</param>
    ///<returns>成功返回true,失败返回false</returns>
    public static bool DeleteXmlAttributeByXPath(string xmlFileName, string xpath, string xmlAttributeName)
    {
        bool isSuccess = false;
        bool isExistsAttribute = false;
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            XmlAttribute xmlAttribute = null;
            if (xmlNode != null)
            {
                //遍历xpath节点中的所有属性
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    if (attribute.Name.ToLower() == xmlAttributeName.ToLower())
                    {
                        //节点中存在此属性
                        xmlAttribute = attribute;
                        isExistsAttribute = true;
                        break;
                    }
                }
                if (isExistsAttribute)
                {
                    //删除节点中的属性
                    xmlNode.Attributes.Remove(xmlAttribute);
                }
            }
            xmlDoc.Save(xmlFileName); //保存到XML文档
            isSuccess = true;
        }
        catch
        {
           // throw ex;
        }
        return isSuccess;
    }

    ///<summary>
    /// 删除匹配XPath表达式的第一个节点中的所有属性
    ///</summary>
    ///<param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
    ///<param name="xpath">要匹配的XPath表达式(例如:"//节点名//子节点名</param>
    ///<returns>成功返回true,失败返回false</returns>
    public static bool DeleteAllXmlAttributeByXPath(string xmlFileName, string xpath)
    {
        bool isSuccess = false;
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(xmlFileName); //加载XML文档
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            if (xmlNode != null)
            {
                //遍历xpath节点中的所有属性
                xmlNode.Attributes.RemoveAll();
            }
            xmlDoc.Save(xmlFileName); //保存到XML文档
            isSuccess = true;
        }
        catch 
        {
           // throw ex; 
        }
        return isSuccess;
    }
    #endregion

}

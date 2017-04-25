using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 行为链构造器
/// </summary>
public class FormulaConstructor
{
    //private IList<> 
    /// <summary>
    /// 构建行为链
    /// </summary>
    /// <param name="info">字符串数据源</param>
    /// <returns>构建完成的行为链</returns>
    public static SkillInfo Constructor(string info)
    {
        // 技能信息
        SkillInfo skillInfo = null;

        if (info != null)
        {
            // 技能ID
            var skillId = 0;
            // 大括号标记
            var braket = false;

            // 解析字符串
            // 根据对应行为列表创建Formula
            var infoLines = info.Split('\n');
            for (var i = 0; i < infoLines.Length; i++)
            {
                var line = infoLines[i];
                // 跳过空行
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                // 消除空格
                line = line.Trim();
                // 跳过注释行
                if (line.StartsWith("//"))
                {
                    continue;
                }

                // 如果是技能描述开始
                if (line.StartsWith("SkillNum"))
                {
                    // 读取技能号
                    var start = line.IndexOf("(", StringComparison.Ordinal);
                    var end = line.IndexOf(")", StringComparison.Ordinal);
                    if (start < 0 || end < 0)
                    {
                        throw new Exception("转换行为链失败: ()符号不完整, 行数:" + (i + 1));
                    }
                    // 编号长度
                    var length = end - start - 1;
                    if (length <= 0)
                    {
                        throw new Exception("转换行为链失败: ()顺序错误, 行数:" + (i + 1));
                    }

                    // 读取技能ID
                    var strSkillId = line.Substring(start + 1, length);
                    skillId = Convert.ToInt32(strSkillId);
                    // 创建新技能
                    skillInfo = new SkillInfo(skillId);
                }
                else if (line.StartsWith("{"))
                {
                    // 开始括号内容
                    braket = true;
                }
                else if (line.StartsWith("}"))
                {
                    // 关闭括号内容
                    braket = false;
                }
                else
                {
                    // 解析内容
                    if (skillInfo != null && braket)
                    {
                        // 参数列表内容
                        var start = line.IndexOf("(", StringComparison.Ordinal);
                        var end = line.IndexOf(")", StringComparison.Ordinal);

                        if (start < 0 || end < 0)
                        {
                            throw new Exception("转换行为链失败: ()符号不完整, 行数:" + (i + 1));
                        }
                        // 编号长度
                        var length = end - start - 1;
                        if (length <= 0)
                        {
                            throw new Exception("转换行为链失败: ()顺序错误, 行数:" + (i + 1));
                        }

                        // 行为类型
                        var type = line.Substring(0, start);
                        // 行为参数
                        var args = line.Substring(start + 1, length);
                        // 消除参数空格
                        args = args.Replace(" ", "");
                        // 使用参数+名称获取IFormula
                        var item = GetFormula(type, args);
                        skillInfo.AddFormulaItem(item);
                    }
                }
            }
        }

        return skillInfo;
    }

    /// <summary>
    /// 获取行为链
    /// </summary>
    /// <param name="type">行为类型名称</param>
    /// <param name="args">行为</param>
    /// <param name="startPos">施法者位置</param>
    /// <param name="targetPos">目标位置</param>
    /// TODO 封装施法者与目标对象
    /// <returns></returns>
    public static IFormulaItem GetFormula(string type, string args)
    {

        IFormulaItem result = null;
        // 错误消息
        string errorMsg = null;
        if (string.IsNullOrEmpty(type))
        {
            errorMsg = "函数类型为空";
        }
        if (string.IsNullOrEmpty(errorMsg))
        {

            var argsArray = args.Split(',');
            var formulaType = Convert.ToInt32(argsArray[0]);
            switch (type)
            {
                case "PointToPoint":
                    { 
                        // 解析参数
                        if (argsArray.Length < 6)
                        {
                            errorMsg = "参数数量错误.需求参数数量:6 实际数量:" + argsArray.Length;
                            break;
                        }
                        // 是否等待完成,特效Key,释放位置(0放技能方, 1目标方),命中位置(0放技能方, 1目标方),速度,飞行轨迹
                        var effectKey = argsArray[1];
                        var releasePos = Convert.ToInt32(argsArray[2]);
                        var receivePos = Convert.ToInt32(argsArray[3]);
                        var speed = Convert.ToSingle(argsArray[4]);
                        var flyType = (TrajectoryAlgorithmType) Enum.Parse(typeof (TrajectoryAlgorithmType), argsArray[5]);
                        // 点对点特效

                        result = new PointToPointFormulaItem(formulaType, effectKey, speed, releasePos, receivePos, flyType);
                    }
                    break;
                case "PointToObj":
                    // 点对对象特效
                    {
                        // 解析参数
                        if (argsArray.Length < 4)
                        {
                            errorMsg = "参数数量错误.需求参数数量:4 实际数量:" + argsArray.Length;
                            break;
                        }
                        // 是否等待完成,特效Key,速度,飞行轨迹
                        var effectKey = argsArray[1];
                        var speed = Convert.ToSingle(argsArray[2]);
                        var flyType = (TrajectoryAlgorithmType)Enum.Parse(typeof(TrajectoryAlgorithmType), argsArray[3]);
                        // 点对点特效

                        result = new PointToObjFormulaItem(formulaType, effectKey, speed, flyType);
                    }
                    break;
                case "Point":
                    // 点特效
                    { 
                        // 解析参数
                        if (argsArray.Length < 5)
                        {
                            errorMsg = "参数数量错误.需求参数数量:4 实际数量:" + argsArray.Length;
                            break;
                        }
                        // 是否等待完成,特效Key,速度,持续时间
                        var effectKey = argsArray[1];
                        var targetPos = Convert.ToInt32(argsArray[2]);
                        var speed = Convert.ToSingle(argsArray[3]);
                        var durTime = Convert.ToSingle(argsArray[4]);
                        result = new PointFormulaItem(formulaType, effectKey, targetPos, speed, durTime);
                    }
                    break;
                case "Scope":
                    // 范围特效
                    // TODO 仔细考虑实现 应该会比较耗
                    break;
                case "CollisionDetection":
                    // 碰撞检测, 二级伤害判断
                {
                        // 参数最低数量
                        var argsCount = 5;
                        // 解析参数
                        if (argsArray.Length < argsCount)
                        {
                            errorMsg = "参数数量错误.需求参数数量最少:5 实际数量:" + argsArray.Length;
                            break;
                        }
                        // 是否等待完成, 目标数量, 目标阵营(-1:都触发, 0: 己方, 1: 非己方), 检测范围形状(0圆, 1方), 
                        // 碰撞单位被释放技能ID, 范围大小(方的就取两个值, 圆的就取第一个值当半径, 有更多的参数都放进来)
                        var targetCount = Convert.ToInt32(argsArray[1]);
                        var targetTypeCamps = (TargetCampsType)Enum.Parse(typeof (TargetCampsType), argsArray[2]);
                        var scopeType = (GraphicType)Enum.Parse(typeof (GraphicType), argsArray[3]);
                        var skillNum = Convert.ToInt32(argsArray[4]);
                        float[] scopeArgs = new float[argsArray.Length - argsCount];
                        // 范围参数
                        for (var i = 0; i < argsArray.Length - argsCount; i++)
                        {
                            scopeArgs[i] = Convert.ToSingle(args[i + argsCount]);
                        }

                        result = new CollisionDetection(formulaType, targetCount, targetTypeCamps, scopeType, scopeArgs, skillNum);
                    }
                    break;
                case "Audio":
                    // 音效
                    {
                        
                    }
                    break;
                case "Buff":
                    // buff
                    {

                    }
                    break;
                //case "Demage":
                //    // 伤害
                //    {
                //        
                //    }
                //    break;
                //case "Cure":
                //    // 治疗
                //    {

                //    }
                //    break;
                case "Calculate":
                    // 结果结算
                    {
                        // TODO 伤害/治疗结算

                    }
                    break;
                default:
                    throw new Exception("未知行为类型: " + type);
            }
        }
        // 如果错误信息不为空则抛出错误
        if (!string.IsNullOrEmpty(errorMsg)) 
        {
            throw new Exception(errorMsg);
        }
        return result;
    }

    // 结构例子
    /*
     SkillNum(10000)
     {
        PointToPoint(1,key,0,1,10,1,1),     // 需要等待其结束, 特效key(对应key,或特效path), 释放位置, 命中位置, 速度10, 飞行轨迹类型
        Point(0,key,1,0,3),                // 不需要等待其结束, 特效key(对应key,或特效path), 释放位置, 播放速度, 持续3秒
        CollisionDetection(1, 1, 10, 0, 10001),
     }
     
     */
    // -----------------特效-------------------- 
    // PointToPoint 点对点特效        参数 是否等待完成,特效Key,释放位置(0放技能方, 1目标方),命中位置(0放技能方, 1目标方),速度,飞行轨迹
    // PointToObj 点对对象特效        参数 是否等待完成,特效Key,速度,飞行轨迹
    // Point 点特效                   参数 是否等待完成,特效Key,速度,持续时间
    // Scope 范围特效                 参数 是否等待完成,特效Key,释放位置(0放技能方, 1目标方),持续时间,范围半径

    // --------------目标选择方式---------------
    // CollisionDetection 碰撞检测    参数 是否等待完成, 目标数量, 检测位置(0放技能方, 1目标方),检测范围形状(0圆, 1方), 范围大小(方的就取两个值, 圆的就取第一个值当半径), 目标阵营(-1:都触发, 0: 己方, 1: 非己方),碰撞单位被释放技能ID
    //{
    //  被释放技能
    //}
    // -----------------音效--------------------
    // Audio 音效                     参数 是否等待完成,点音,持续音,持续时间

    // -----------------buff--------------------
    // Buff buff                      参数 是否等待完成,buffID

    // -----------------结算--------------------
    // Calculate 结算                 参数 是否等待完成,伤害,治疗,目标数据,技能数据


}

/// <summary>
/// 技能信息
/// </summary>
public class SkillInfo
{
    /// <summary>
    /// 技能ID
    /// </summary>
    public int SkillNum { get; private set; }

    /// <summary>
    /// 技能行为单元列表
    /// </summary>
    private List<IFormulaItem> formulaItemList = new List<IFormulaItem>();

    /// <summary>
    /// 构造技能信息
    /// </summary>
    /// <param name="skillNum">技能ID</param>
    public SkillInfo(int skillNum)
    {
        SkillNum = skillNum;
    }

    /// <summary>
    /// 添加行为生成器
    /// </summary>
    /// <param name="formulaItem">行为单元生成器</param>
    public void AddFormulaItem(IFormulaItem formulaItem)
    {
        if (formulaItem == null)
        {
            return;
        }
        formulaItemList.Add(formulaItem);
    }

    /// <summary>
    /// 获取行为链
    /// </summary>
    /// <param name="paramsPacker">参数封装</param>
    /// <returns>行为链</returns>
    public IFormula GetFormula(FormulaParamsPacker paramsPacker)
    {
        if (paramsPacker == null)
        {
            throw new Exception("参数封装为空.");
        }
        IFormula result = null;

        // 设置技能ID
        paramsPacker.SkillNum = SkillNum;

        foreach (var item in formulaItemList)
        {
            if (result != null)
            {
                result = result.After(item.GetFormula(paramsPacker));
            }
            else
            {
                result = item.GetFormula(paramsPacker);
            }
        }

        if (result != null)
        {
            result = result.GetFirst();
        }
        return result;
    }
}

/// <summary>
/// 碰撞检测器
/// </summary>
public class CollisionDetection : IFormulaItem
{
    // 使用目标选择器选择范围内适合的单位
    
    /// <summary>
    /// 行为类型
    /// 0: 不等待其执行结束继续
    /// 1: 等待期执行结束调用callback
    /// </summary>
    public int FormulaType { get; private set; }

    /// <summary>
    /// 选取目标数量上限
    /// </summary>
    public int TargetCount { get; private set; }

    /// <summary>
    /// 目标阵营
    /// </summary>
    public TargetCampsType TargetCamps { get; private set; }

    /// <summary>
    /// 检测范围形状
    /// </summary>
    public GraphicType ScopeType { get; private set; }

    /// <summary>
    /// 范围描述参数
    /// </summary>
    public float[] ScopeParams { get; private set; }

    /// <summary>
    /// 技能ID
    /// </summary>
    public int SkillNum { get; private set; }

    /// <summary>
    /// 初始化碰撞检测
    /// </summary>
    /// <param name="formulaType">行为单元类型(0: 不等待, 1: 等待)</param>
    /// <param name="targetCount">目标数量</param>
    /// <param name="targetCamps">目标阵营</param>
    /// <param name="scopeType">范围类型</param>
    /// <param name="scopeParams">范围参数</param>
    /// <param name="skillNum">释放技能ID</param>
    public CollisionDetection(int formulaType, int targetCount, TargetCampsType targetCamps, GraphicType scopeType, float[] scopeParams, int skillNum)
    {
        this.FormulaType = formulaType;
        this.TargetCount = targetCount;
        this.TargetCamps = targetCamps;
        this.ScopeType = scopeType;
        this.ScopeParams = scopeParams;
        this.SkillNum = skillNum;
    }

    /// <summary>
    /// 生成行为单元
    /// </summary>
    /// <returns>行为单元对象</returns>
    public IFormula GetFormula(FormulaParamsPacker paramsPacker)
    {
        IFormula result = null;


        // TODO 技能数据应该在paramsPacker中

        // 检测范围
        // 获取图形对象

        // 搜索位置,形状(包含大小),目标数量,释放技能ID
        // 检查范围内对象
        // 获取目标单位个数个单位
        // 对他们释放技能(技能编号)
        // 


        return result;
    }
}

/// <summary>
/// 目标阵营类型
/// </summary>
public enum TargetCampsType
{
    All = -1,
    Same = 0,
    Different = 1
}

/// <summary>
/// 碰撞检测图形接口
/// </summary>
public interface ICollisionGraphics
{
    /// <summary>
    /// 获取图形类型
    /// </summary>
    /// <returns></returns>
    GraphicType GraphicType { get; set; }

    /// <summary>
    /// 图形所在位置
    /// </summary>
    Vector2 Postion { get; set; }

    /// <summary>
    /// 检测与其他图形的碰撞
    /// </summary>
    /// <param name="graphics">其他图形对象</param>
    /// <returns></returns>
    bool CheckCollision(ICollisionGraphics graphics);
}


public abstract class CollisionGraphics : ICollisionGraphics
{

    /// <summary>
    /// 获取图形类型
    /// </summary>
    public GraphicType GraphicType
    {
        get { return graphicType; }
        set { graphicType = value; }
    }

    /// <summary>
    /// 图形所在位置
    /// </summary>
    public Vector2 Postion { get; set; }


    /// <summary>
    /// 图形类型
    /// </summary>
    private GraphicType graphicType;


    /// <summary>
    /// 检测与其他图形的碰撞
    /// </summary>
    /// <param name="graphics">其他图形对象</param>
    /// <returns>是否碰撞</returns>
    public bool CheckCollision(ICollisionGraphics graphics)
    {
        return false;
    }

    /// <summary>
    /// 检测是否碰撞
    /// </summary>
    /// <param name="item1"></param>
    /// <param name="item2"></param>
    /// <returns></returns>
    public static bool CheckCollision(ICollisionGraphics item1, ICollisionGraphics item2)
    {
        var result = false;
        switch (item1.GraphicType)
        {
            case GraphicType.Circle:
                switch (item1.GraphicType)
                {
                    case GraphicType.Circle:
                        break;
                    // 检测1 半径和是否大于距离
                    case GraphicType.Rect:
                        break;
                    // 检测2 半径到四边距离
                    case GraphicType.Sector:
                        break;
                        // 检测3 圆与扇形
                }
                break;

            case GraphicType.Rect:
                switch (item1.GraphicType)
                {
                    case GraphicType.Circle:
                        break;
                    // 检测2 半径到四边距离
                    case GraphicType.Rect:
                        break;
                    // 检测4 矩形与矩形
                    case GraphicType.Sector:
                        break;
                        // 检测5 矩形与扇形
                }
                break;

            case GraphicType.Sector:
                switch (item1.GraphicType)
                {
                    case GraphicType.Circle:
                        break;
                    // 检测3 扇形与圆
                    case GraphicType.Rect:
                        break;
                    // 检测5 扇形与矩形
                    case GraphicType.Sector:
                        break;
                        // 检测6 扇形与扇形
                }
                break;

        }
        return result;
    }


    /// <summary>
    /// 圆圆碰撞检测
    /// </summary>
    /// <param name="circle1">圆1</param>
    /// <param name="circle2">圆2</param>
    /// <returns>是否碰撞</returns>
    public static bool CheckCircleAndCircle(ICollisionGraphics circle1, ICollisionGraphics circle2)
    {
        var result = false;

        var circleGraphics1 = circle1 as CircleGraphics;
        var circleGraphics2 = circle2 as CircleGraphics;

        if (circleGraphics1 != null && circleGraphics2 != null)
        {
            // 检查两圆的半径与距离大小
            result = (circleGraphics1.Postion - circleGraphics2.Postion).magnitude <
                     circleGraphics1.Radius + circleGraphics2.Radius;
        }

        return result;
    }

    /// <summary>
    /// 圆方碰撞检测
    /// </summary>
    /// <param name="circle">圆</param>
    /// <param name="rect">方</param>
    /// <returns>是否碰撞</returns>
    public static bool CheckCircleAndRect(ICollisionGraphics circle, ICollisionGraphics rect)
    {
        // 转换格式
        var circleGraphics = circle as CircleGraphics;
        var rectGraphics = rect as RectGraphics;

        if (circleGraphics != null && rectGraphics != null)
        {
            // 计算两轴长度
            var halfWidth = rectGraphics.Width*0.5f;
            var halfHeight = rectGraphics.Height*0.5f;
            var raduis = circleGraphics.Radius;
            //var xAxisLength = Utils.GetDistance(rectGraphics.Postion, rectGraphics.Postion + new Vector2());

            // 右侧中心点相对向量
            var pointRightCenter = new Vector2((float)Math.Cos(rectGraphics.Rotation) * halfWidth, (float)Math.Sin(rectGraphics.Rotation) * halfWidth);
            // 上侧中心点相对向量
            var pointTopCenter = new Vector2((float)Math.Sin(rectGraphics.Rotation) * halfHeight, (float)Math.Cos(rectGraphics.Rotation) * halfHeight);

            var toRightDistance = Utils.GetDistancePointToLine(rectGraphics.Postion, rectGraphics.Postion + pointRightCenter, circle.Postion);
            var toTopDistance = Utils.GetDistancePointToLine(rectGraphics.Postion, rectGraphics.Postion + pointTopCenter, circle.Postion);

            if (toRightDistance > halfWidth + raduis)
            {
                return false;
            }
            if (toTopDistance > halfHeight + raduis)
            {
                return false;
            }

            if (toRightDistance < halfWidth)
            {
                return true;
            }
            if (toTopDistance < halfHeight)
            {
                return true;
            }

            var distanceX = toRightDistance - halfWidth;
            var distanceY = toTopDistance - halfHeight;
            return (distanceX*distanceX + distanceY*distanceY) <= raduis*raduis;
        }

        return false;
    }

    /// <summary>
    /// 扇圆碰撞检测
    /// </summary>
    /// <param name="sector">扇形</param>
    /// <param name="circle">圆形</param>
    /// <returns></returns>
    public static bool CheckSectorAndCircle(ICollisionGraphics sector, ICollisionGraphics circle)
    {
        var result = false;

        var sectorGraphics = sector as SectorGraphics;
        var circleGraphics = circle as CircleGraphics;
        if (sectorGraphics != null && circleGraphics != null)
        {
            
        }


        return result;
    }

    /// <summary>
    /// 方方碰撞检测
    /// </summary>
    /// <param name="rect1">方形1</param>
    /// <param name="rect2">方形2</param>
    /// <returns>是否碰撞</returns>
    public static bool CheckRectAndRect(ICollisionGraphics rect1, ICollisionGraphics rect2)
    {

        // 转换类型
        var rectGraphics1 = rect1 as RectGraphics;
        var rectGraphics2 = rect2 as RectGraphics;

        if (rectGraphics1 != null && rectGraphics2 != null)
        {
            var axisArray = new[]
            {
                rectGraphics1.HorizonalAxis,
                rectGraphics1.VerticalAxis,
                rectGraphics2.HorizonalAxis,
                rectGraphics2.VerticalAxis
            };

            for (var i = 0; i < axisArray.Length; i++)
            {
                var axis = axisArray[i];
                if (
                    RectGraphics.GetProjectionRaduis(axis, rectGraphics1.Width, rectGraphics1.Height,
                        rectGraphics1.HorizonalAxis, rectGraphics1.VerticalAxis) +
                    RectGraphics.GetProjectionRaduis(axis, rectGraphics2.Width, rectGraphics2.Height,
                        rectGraphics2.HorizonalAxis, rectGraphics2.VerticalAxis) <=
                    Vector2.Dot(rectGraphics1.Postion - rectGraphics2.Postion, axis))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 扇方碰撞检测
    /// </summary>
    /// <param name="sector">扇形</param>
    /// <param name="rect">方形</param>
    /// <returns>是否碰撞</returns>
    public static bool CheckSectorAndRect(ICollisionGraphics sector, ICollisionGraphics rect)
    {
        var result = false;



        return result;
    }

    /// <summary>
    /// 扇形扇形碰撞检测
    /// </summary>
    /// <param name="sector1">扇形1</param>
    /// <param name="sector2">扇形2</param>
    /// <returns>是否碰撞</returns>
    public static bool CheckSectorAndSector(ICollisionGraphics sector1, ICollisionGraphics sector2)
    {
        var result = false;



        return result;
    }
}

/// <summary>
/// 矩形图形
/// </summary>
public class RectGraphics : CollisionGraphics
{
    
    /// <summary>
    /// 举行宽度
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// 举行高度
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// 旋转角度
    /// </summary>
    public float Rotation {
        get { return rotation; }
        set
        {
            rotation = value;
            if (rotation > 360 || rotation < 360)
            {
                rotation %= 360;
            }

            // 重置相对水平轴与相对垂直轴
            HorizonalAxis = Utils.GetHorizonalTestLine(rotation);
            VerticalAxis = Utils.GetVerticalTextLine(rotation);
        }
    }


    /// <summary>
    /// 相对水平轴
    /// </summary>
    public Vector2 HorizonalAxis { get; private set; }

    /// <summary>
    /// 相对垂直轴
    /// </summary>
    public Vector2 VerticalAxis { get; private set; }

    /// <summary>
    /// 图形类型
    /// </summary>
    private GraphicType graphicType;

    /// <summary>
    /// 旋转角度
    /// </summary>
    private float rotation = 0f;


    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="rotation">旋转角度</param>
    public RectGraphics(Vector2 position, float width, float height, float rotation)
    {
        Rotation = rotation;
        Postion = position;
        Width = width;
        Height = height;
    }


    /// <summary>
    /// 获取Axis2映射到Axis1的值
    /// </summary>
    /// <param name="axis">映射轴</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="horizonalAxis">横向轴</param>
    /// <param name="verticalAxis">纵向轴</param>
    /// <returns>映射值</returns>
    public static float GetProjectionRaduis(Vector2 axis, float width, float height, Vector2 horizonalAxis, Vector2 verticalAxis)
    {
        // 加入旋转
        var halfWidth = width / 2;
        var halfHeight = height / 2;
        float projectionAxisX = Vector2.Dot(axis, horizonalAxis);
        float projectionAxisY = Vector2.Dot(axis, verticalAxis);

        return halfWidth * projectionAxisX + halfHeight * projectionAxisY;
    }
}


/// <summary>
/// 圆形
/// </summary>
public class CircleGraphics : ICollisionGraphics
{

    /// <summary>
    /// 获取图形类型
    /// </summary>
    public GraphicType GraphicType
    {
        get { return graphicType; }
        set { graphicType = value; }
    }

    /// <summary>
    /// 图形所在位置
    /// </summary>
    public Vector2 Postion { get; set; }
    

    /// <summary>
    /// 旋转角度
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// 半径
    /// </summary>
    public float Radius { get; set; }


    /// <summary>
    /// 图形类型
    /// </summary>
    private GraphicType graphicType;


    /// <summary>
    /// 检测与其他图形的碰撞
    /// </summary>
    /// <param name="graphics">其他图形对象</param>
    /// <returns>是否碰撞</returns>
    public bool CheckCollision(ICollisionGraphics graphics)
    {
        return false;
    }
}



public class SectorGraphics : ICollisionGraphics
{

    /// <summary>
    /// 获取图形类型
    /// </summary>
    public GraphicType GraphicType
    {
        get { return graphicType; }
        set { graphicType = value; }
    }

    /// <summary>
    /// 图形所在位置
    /// </summary>
    public Vector2 Postion { get; set; }


    /// <summary>
    /// 旋转角度
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// 半径
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// 扇形打开角度
    /// 0-360
    /// </summary>
    public float OpenAngle { get; set; }


    /// <summary>
    /// 图形类型
    /// </summary>
    private GraphicType graphicType;


    /// <summary>
    /// 检测与其他图形的碰撞
    /// </summary>
    /// <param name="graphics">其他图形对象</param>
    /// <returns>是否碰撞</returns>
    public bool CheckCollision(ICollisionGraphics graphics)
    {
        return false;
    }
}
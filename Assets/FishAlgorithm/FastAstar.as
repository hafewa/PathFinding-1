package
{
	
	/**
	 * A*搜索 128x128 4方向
	 * @author clifford.cheny http://www.cnblogs.com/flash3d/
	 */
	public class FastAstar
	{	
		public static const MAP_SIZE:int = 128;
		public static const MAP_SIZE_1:int = 127;
		public static const BLOCK_NUM:int = 1000;
		public static const D1:int = 7;
		
		//public var tests:Array;
		
		public var map:Vector.<Node>;
		public var path:Array;
		public var nodeVersion:int = 0;
		public var mapVersion:int = 0;
		public var isPrune:Boolean = true;
		
		//产生地图
		public function createMap():void
		{
			mapVersion++;
			
			var l:int;
			var i:int;
			var r:int;
			var x:int;
			var y:int;
			var n:Node;
			var links:Vector.<Node>;
			var c:int;
			
			if (!map)
			{
				map = new Vector.<Node>(MAP_SIZE * MAP_SIZE);
				l = map.length;
				for (i = 0; i < l; i++)
				{
					n = new Node();
					x = i & MAP_SIZE_1;
					y = i >> D1;//666 用位移作除法, 速度快很多
					n.x = x;
					n.y = y;
					n.links = new Vector.<Node>(4);// 上下左右的链接, 地图预处理
					map[i] = n;
				}
			}
			l = map.length;
			i = 0;
			while (i < BLOCK_NUM)
			{
				r = int(Math.random() * l);
				if (map[r].block === mapVersion) continue;
				map[r].block = mapVersion;
				i++;
			}
			// 遍历地图
			for (i = 0; i < l; i++)
			{
				n = map[i];
				if (n.block === mapVersion) continue;
				links = n.links;
				x = n.x;
				y = n.y;
				c = 0;

				// 这个判断区分四轴格子加入队列的顺序
				// 获取间隔的格子(交叉相间斜线格子)
				if (((i & MAP_SIZE) >> D1) ^ (i & 1))
				{
					// 666 用或操作替代乘, 速度快

					// 上
					if (y > 0 && map[(y - 1) << D1 | x].block !== mapVersion)
					{
						links[c] = map[(y - 1) << D1 | x];
						c++;
					}
					// 下
					if (y < MAP_SIZE_1 && map[(y + 1) << D1 | x].block !== mapVersion)
					{
						links[c] = map[(y + 1) << D1 | x];
						c++;
					}
					// 右
					if (x < MAP_SIZE_1 && map[y << D1 | (x + 1)].block !== mapVersion)
					{
						links[c] = map[y << D1 | (x + 1)];
						c++;
					}
					// 左
					if (x > 0 && map[y << D1 | (x - 1)].block !== mapVersion)
					{
						links[c] = map[y << D1 | (x - 1)];
						c++;
					}
				}
				// 斜线以外的格子
				else
				{
					// 右
					if (x < MAP_SIZE_1 && map[y << D1 | (x + 1)].block !== mapVersion)
					{
						links[c] = map[y << D1 | (x + 1)];
						c++;
					}
					// 左
					if (x > 0 && map[y << D1 | (x - 1)].block !== mapVersion)
					{
						links[c] = map[y << D1 | (x - 1)];
						c++;
					}
					// 上
					if (y > 0 && map[(y - 1) << D1 | x].block !== mapVersion)
					{
						links[c] = map[(y - 1) << D1 | x];
						c++;
					}
					// 下
					if (y < MAP_SIZE_1 && map[(y + 1) << D1 | x].block !== mapVersion)
					{
						links[c] = map[(y + 1) << D1 | x];
						c++;
					}
				}
				n.linksLength = c;
			}
		}
		
		//搜索
		public function search(startX:int, startY:int, endX:int, endY:int):void
		{
			//tests = [];
			
			var startNode:Node = map[startY << D1 | startX];
			var endNode:Node = map[endY << D1 | endX];
			// 起始点与结束点无效
			if (startNode.block === mapVersion || endNode.block === mapVersion)
			{
				path = [];
				return;
			}
			var i:int;
			var l:int;
			var f:int;
			var t:Node;
			var openBase:int = Math.abs(endX - startX) + Math.abs(endY - startY); // 到目标预估代价值
			var open:Vector.<Node> = new Vector.<Node>(2); // 开放路径列表
			var current:Node; // 当前节点
			var test:Node; // 周围节点
			var links:Vector.<Node>; // 周围节点列表
			open[0] = startNode; // 开放列表加入起始节点
			startNode.pre = startNode.next = null;
			startNode.version = ++nodeVersion;
			startNode.nowCost = 0; // 初始化当前移动代价
			while (true)
			{
				current = open[0];
				open[0] = current.next;
				if (open[0]) open[0].pre = null;
				
				//tests[tests.length] = current;
				
				// 搜索结束
				if (current === endNode)
				{
					if (isPrune) prunePath(startNode, current);
					else buildPath(startNode, current);
					return;//查询到路径, 退出
				}
				// 说去周围节点
				links = current.links;
				// 周围节点数量
				l = current.linksLength;
				// 遍历周围节点
				for (i = 0; i < l; i++)
				{
					test = links[i];
					// 当前花费+1
					f = current.nowCost + 1;
					// 如果不是障碍物
					if (test.version !== nodeVersion)
					{
						// 记录已搜索
						test.version = nodeVersion;
						// 设置父级
						test.parent = current;
						// 设置当前花费
						test.nowCost = f;
						// 设置距离
						test.dist = Math.abs(endX - test.x) + Math.abs(endY - test.y);
						f += test.dist;
						// 设置预计花费
						test.mayCost = f;
						// 对比花费
						f = (f - openBase) >> 1;
						test.pre = null;
						test.next = open[f];
						if (open[f]) open[f].pre = test;
						open[f] = test;
					}
					// 如果当前节点花费大于预估
					else if (test.nowCost > f)
					{
						// 删除节点
						if (test.pre) test.pre.next = test.next;
						if (test.next) test.next.pre = test.pre;
						if (open[1] === test) open[1] = test.next;
						test.parent = current;
						test.nowCost = f;
						test.mayCost = f + test.dist;
						test.pre = null;
						test.next = open[0];
						if (open[0]) open[0].pre = test;
						open[0] = test;
					}
				}
				if (!open[0])
				{
					// 路径不存在
					if (!open[1]) break;
					t = open[0];
					open[0] = open[1];
					open[1] = t;
					openBase += 2;
				}
			}
			path = [];
			return;//返回路径
		}
		
		private function prunePath(startNode:Node, endNode:Node):void
		{
			path = [endNode];
			if (endNode === startNode) return;
			var current:Node = endNode.parent;
			var dx:int = current.x - endNode.x;
			var dy:int = current.y - endNode.y;
			var cx:int;
			var cy:int;
			var t:Node;
			var t2:Node;
			
			while (true)
			{
				if (current === startNode)
				{
					path[path.length] = current;
					return;
				}
				t = current.parent;
				cx = current.x;
				cy = current.y;
				// 路径遍历未结束
				if (t !== startNode)
				{
					t2 = t.parent
					// 两点之间无障碍则合并
					if (Math.abs(t2.x - cx) === 1 && Math.abs(t2.y - cy) === 1 && map[cy << D1 | t2.x].block !== mapVersion && map[t2.y << D1 | cx].block !== mapVersion)
					{
						t = t2;
					}
				}
				if (t.x -  cx === dx && t.y -  cy === dy)
				{
					current = t;
				}
				else
				{
					dx = t.x -  cx;
					dy = t.y -  cy;
					path[path.length] = current;
					current = t;
				}
			}
		}
		
		private function buildPath(startNode:Node, endNode:Node):void
		{
			path = [endNode];
			while (endNode !== startNode)
			{
				endNode = endNode.parent;
				path[path.length] = endNode;
			}
		}
	}
}
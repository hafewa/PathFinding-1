Shader "Custom/ClipScroll" {
	Properties{

		// 序列 材质
		// 设置该材质(该材质应该包含多个微变化多帧图片进行位移产生序列帧效果)
		_MainTex("Base (RGB)", 2D) = "white" {}
	// 高亮颜色
	_RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		// 材质中包含图像个数
		// 横向帧数
		_XCellAmount("X Cell Amount", float) = 0.0
		// 纵向帧数
		_YCellAmount("Y Cell Amount", float) = 0.0
		// 总帧数(如5*5的帧数此处填21或23 则循环从1至21或23帧的位置)
		_AllCellAmount("All Cell Amount", float) = 0.0
		// 图像变动次数
		// 帧变速度
		//_Speed("Speed", Range(0.01, 200)) = 20
		_Position("Position", float) = 0.0

		// 左侧建材范围, 0不剪裁, 1全剪裁
		_LeftClip("LeftClip", Range(0, 1)) = 0.0
		// 右侧建材范围, 0不剪裁, 1全剪裁
		_RightClip("RightClip", Range(0, 1)) = 0.0
		// 上侧建材范围, 0不剪裁, 1全剪裁
		_TopClip("TopClip", Range(0, 1)) = 0.0
		// 下侧建材范围, 0不剪裁, 1全剪裁
		_BottomClip("BottomClip", Range(0, 1)) = 0.0
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" }

		Lighting On
		CGPROGRAM
#pragma surface surf Lambert alpha
	sampler2D _MainTex;
	float4 _RimColor;
	float _XCellAmount;
	float _YCellAmount;
	float _AllCellAmount;
	//float _Speed;
	float _Position;
	float _LeftClip;
	float _RightClip;
	float _TopClip;
	float _BottomClip;

	struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutput o) {
		bool isHighLight = false;
		float2 spriteUV = IN.uv_MainTex;
		// 获取每个单元宽度所占百分比
		float xCellUIPercentage = 1.0f / _XCellAmount;
		float yCellUIPercentage = 1.0f / _YCellAmount;
		// 获取当前时间
		//float timeSpeed = _Time.y * _Speed;
		float allCount = fmod(_Position, _AllCellAmount);
		// 排除0图显示, 部分显卡会闪烁
		if (allCount == 0) {
			allCount = 1;
		}
		// 设置时间X轴偏移
		float timeValX = fmod(allCount, _XCellAmount);
		timeValX = ceil(timeValX);

		//设置
		int timeValY = _YCellAmount - allCount / _XCellAmount;
		// 设置当前显示位置X值
		float xValue = spriteUV.x;
		/*if (_XCellAmount != 1) {
			xValue += timeValX - 1;
		}*/
		//xValue *= xCellUIPercentage;

		// 设置当前显示位置Y值
		float yValue = spriteUV.y;
		/*if (_YCellAmount != 1) {
			yValue += timeValY;
		}*/
		//yValue *= yCellUIPercentage;

		// spriteUV = float2(xValue, yValue);

		// 如果当前位置在高亮区域, 则提高亮度

		float xr = (timeValX)* xCellUIPercentage - (1.0f / _XCellAmount * _RightClip);
		float xl = (timeValX - 1) * xCellUIPercentage + (1.0f / _XCellAmount * _LeftClip);
		float yt = (timeValY + 1) * yCellUIPercentage - (1.0f / _YCellAmount * _TopClip);
		float yb = (timeValY)* yCellUIPercentage + (1.0f / _YCellAmount * _BottomClip);

		// 判断剪裁范围
		if (!(xValue > xl && xValue < xr && yValue > yb && yValue < yt)) {
			isHighLight = true;
		}
		// -------------------------解决模糊版本-------------------------------

		//int index = floor(_Time.x * _Speed);
		//int indexY = index / _XCellAmount;
		//int indexX = index - indexY * _XCellAmount;
		//float2 uv = float2(IN.uv_MainTex.x / _XCellAmount, IN.uv_MainTex.y / _YCellAmount);
		//uv.x += indexX / _XCellAmount;
		//uv.y -= indexY / _YCellAmount;


		half4 c = tex2D(_MainTex, spriteUV);

		if (!isHighLight) {
			c.rgb = c.rgb * _RimColor.rgb;
		}
		// o.Albedo = c.rgb; // 漫反射在手机上会有问题(会显示黑色)
		o.Emission = c.rgb;
		o.Alpha = c.a;
	}
	ENDCG
	}
		FallBack "Diffuse"
	}
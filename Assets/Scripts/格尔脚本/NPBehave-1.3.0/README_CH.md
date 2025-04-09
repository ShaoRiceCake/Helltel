## 鍓嶈█
NPBahave鏄疓itHub涓婂紑婧愮殑涓€涓�琛屼负鏍戯紝鍏朵唬鐮佺畝娲佹湁鍔涳紝涓嶶nity鑰﹀悎杈冧綆锛孾toc]閫傚悎鎷挎潵鍋氬弻绔�琛屼负鏍戙€俙娉ㄦ剰锛岀敱浜庢椂闂村叧绯伙紝鍘熸枃涓�鐨勯摼鎺ヨ繖閲屽皢涓嶅啀鎻愪緵寮曠敤銆俙
## 寮€婧愰摼鎺�
[https://github.com/meniku/NPBehave](https://github.com/meniku/NPBehave)
## 姝ｆ枃
![NPBehave Logo](http://labs.nkuebler.de/npbehave/images/np-behave.png)
NPBehave鑷村姏浜庯細

- 杞婚噺锛屽揩閫燂紝绠€娲�
- 浜嬩欢椹卞姩
- 鏄撲簬鎷撳睍
- 涓€涓�鐢ㄤ唬鐮佸畾涔堿I琛屼负鐨勬�嗘灦锛岀洰鍓嶆病鏈夊彲瑙嗗寲缂栬緫鍣ㄦ敮鎸侊紙`鏈�浜哄皢涓哄叾璐＄尞涓€涓猔锛�

NPBehave鍩轰簬鍔熻兘寮哄ぇ涓旂伒娲荤殑鍩轰簬浠ｇ爜鐨勬柟娉曪紝浠巄ehavior搴撳畾涔夎�屼负鏍戯紝骞舵贩鍚堜簡铏氬够寮曟搸鐨勪竴浜涘緢妫掔殑琛屼负鏍戞�傚康銆備笌浼犵粺鐨勮�屼负鏍戜笉鍚岋紝浜嬩欢椹卞姩鐨勮�屼负鏍戜笉闇€瑕佹瘡甯т粠鏍硅妭鐐归亶鍘嗐€傚畠浠�淇濇寔褰撳墠鐘舵€侊紝鍙�鏈夊湪瀹為檯闇€瑕佹椂鎵嶇户缁�閬嶅巻銆傝繖浣垮緱瀹冧滑鐨勬€ц兘鏇撮珮锛屼娇鐢ㄨ捣鏉ヤ篃鏇寸畝鍗曘€�

鍦∟PBehave涓�锛屾偍灏嗗彂鐜板ぇ澶氭暟鑺傜偣绫诲瀷鏉ヨ嚜浼犵粺鐨勮�屼负鏍戯紝浣嗕篃鏈変竴浜涚被浼间簬铏氬够寮曟搸涓�鐨勮妭鐐圭被鍨嬨€備笉杩囷紝娣诲姞鎮ㄨ嚜宸辩殑鑷�瀹氫箟鑺傜偣绫诲瀷涔熺浉褰撳�规槗銆�

### 瀹夎��
鍙�闇€灏哊PBehave鏂囦欢澶规斁鍏�Unity椤圭洰涓�銆傝繕鏈変竴涓狤xamples瀛愭枃浠跺す锛屽叾涓�鏈変竴浜涙偍鍙�鑳芥兂瑕佸弬鑰冪殑绀轰緥鍦烘櫙銆�

### 渚嬪瓙锛氣€淗ello World鈥� 琛屼负鏍�
璁╂垜浠�寮€濮嬩竴涓�渚嬪瓙
```csharp
using NPBehave;

public class HelloWorld : MonoBehaviour
{
    private Root behaviorTree;

    void Start()
    {
        behaviorTree = new Root(
            new Action(() => Debug.Log("Hello World!"))
        );
        behaviorTree.Start();
    }
}
```
褰撴偍杩愯�屾�ゅ懡浠ゆ椂锛屾偍灏嗘敞鎰忓埌鈥淗ello World鈥濆皢琚�涓€娆″張涓€娆″湴鎵撳嵃鍑烘潵銆傝繖鏄�鍥犱负褰撻亶鍘嗚繃鏍戜腑鐨勬渶鍚庝竴涓�鑺傜偣鏃讹紝鏍硅妭鐐瑰皢閲嶆柊鍚�鍔ㄦ暣涓�鏍戙€傚�傛灉涓嶉渶瑕佽繖鏍凤紝鍙�浠ユ坊鍔犱竴涓獁aituntilstop鑺傜偣锛屽�備笅鎵€绀�:
```csharp
// ...
behaviorTree = new Root(
	new Sequence(
		new Action(() => Debug.Log("Hello World!")),
		new WaitUntilStopped()
	)
);
///... 
```
鍒扮洰鍓嶄负姝�锛岃繖涓�琛屼负鏍戜腑杩樻病鏈変换浣曚簨浠堕┍鍔ㄣ€傚湪鎴戜滑娣卞叆鐮旂┒涔嬪墠锛屾偍闇€瑕佷簡瑙ｉ粦鏉匡紙Blackboards锛夋槸浠€涔堛€�

### Blackboards锛堥粦鏉匡級
鍦∟PBehave涓�锛屽氨鍍忓湪铏氬够寮曟搸涓�涓€鏍凤紝鎴戜滑鏈夐粦鏉裤€備綘鍙�浠ユ妸瀹冧滑鐪嬩綔鏄�浣犵殑AI鐨勨€滆�板繂鈥濄€傚湪NPBehave涓�锛岄粦鏉挎槸鍩轰簬鍙�浠ヨ�傚療鏇存敼鐨勫瓧鍏搞€傛垜浠�涓昏�佷娇鐢╜Service`鏉ュ瓨鍌ㄥ拰鏇存柊榛戞澘涓�鐨勫€笺€傛垜浠�浣跨敤`BlackboardCondition`鎴朻BlackboardQuery`鏉ヨ�傚療榛戞澘鐨勫彉鍖栵紝鐒跺悗閬嶅巻bahaviour鏍戙€傛偍涔熷彲浠ュ湪鍏朵粬浠讳綍鍦版柟璁块棶鎴栦慨鏀归粦鏉跨殑鍊�(鎮ㄤ篃鍙�浠ョ粡甯镐粠Action鑺傜偣璁块棶瀹冧滑)銆�

褰撴偍瀹炰緥鍖栦竴涓猔鏍癸紙Root锛塦鏃讹紝榛戞澘灏嗚嚜鍔ㄥ垱寤猴紝浣嗘槸鎮ㄤ篃鍙�浠ヤ娇鐢ㄥ畠鐨勬瀯閫犲嚱鏁版彁渚涘彟涓€涓�瀹炰緥(杩欏�逛簬`鍏变韩榛戞澘锛圫hared Blackboards锛塦鐗瑰埆鏈夌敤)

### 渚嬪瓙锛氫竴涓�浜嬩欢椹卞姩鐨勮�屼负鏍�
杩欐湁涓€涓�浣跨敤榛戞澘鐨勪簨浠堕┍鍔ㄧ殑琛屼负鏍戜緥瀛�
```csharp
/// ...
behaviorTree = new Root(
    new Service(0.5f, () => { behaviorTree.Blackboard["foo"] = !behaviorTree.Blackboard.Get<bool>("foo"); },
        new Selector(
        
            new BlackboardCondition("foo", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new Action(() => Debug.Log("foo")),
                    new WaitUntilStopped()
                )
            ),

            new Sequence(
                new Action(() => Debug.Log("bar")),
                new WaitUntilStopped()
            )
        )
    )
);
behaviorTree.Start();
//...
```
杩欎釜绀轰緥灏嗗湪姣�500姣�绉掍氦鏇挎墦鍗扳€渇oo鈥濆拰鈥渂ar鈥濄€傛垜浠�浣跨敤涓€涓猔鏈嶅姟`瑁呴グ鍣ㄨ妭鐐瑰湪榛戞澘涓婂垏鎹�foo boolean鍊笺€傛垜浠�浣跨敤BlackboardCondition瑁呴グ鍣ㄨ妭鐐规牴鎹�杩欎釜boolean鍊兼潵鍐冲畾鏄�鍚︽墽琛屽垎鏀�銆侭lackboardCondition杩樹細鏍规嵁杩欎釜鍊肩洃瑙嗛粦鏉跨殑鍙樺寲锛堜緷鎹�榛戞澘鐨勫綋鍓嶅€煎拰鎴戜滑鎻愪緵鐨勫€煎仛涓哄垽鏂�鍩哄噯锛夛紝`Stops.IMMEDIATE_RESTART`浣滅敤鏄�濡傛灉鏉′欢涓嶅啀涓虹湡锛屽垯褰撳墠鎵ц�岀殑鍒嗘敮灏嗗仠姝�锛屽�傛灉鏉′欢鍐嶆�′负鐪燂紝鍒欑珛鍗抽噸鏂板惎鍔ㄣ€�

璇锋敞鎰忥紝鎮ㄥ簲璇ュ皢鏈嶅姟鏀惧湪鐪熸�ｇ殑鏂规硶涓�锛岃€屼笉鏄�浣跨敤lambdas锛岃繖灏嗕娇鎮ㄧ殑鏍戞洿鍏峰彲璇绘€с€傛洿澶嶆潅鐨勮�屼负涔熸槸濡傛�ゃ€�

### 缁堟�㈠師鍒�
涓€浜涜�呴グ鍣�(濡侭lackboardCondition銆丆ondition鎴朆lackboardQuery)鏈変竴涓猻topsOnChange鍙傛暟锛屽厑璁稿畾涔塻top瑙勫垯銆傝�ュ弬鏁板厑璁歌�呴グ鍣ㄥ仠姝㈠叾鐖禶缁勫悎锛圕omposite锛塦涓�姝ｅ湪杩愯�岀殑瀛愭爲銆備粬鏄�鎮ㄧ敤鏉ユ帉鎺�NPBehave涓�鐨勪簨浠堕┍鍔ㄧ殑涓昏�佸伐鍏枫€�

杈僠浣庝紭鍏堢骇`鐨勮妭鐐规槸鍦ㄥ叾鐖禶缁勫悎`涓�鐨勫綋鍓嶈妭鐐逛箣鍚庡畾涔夌殑鑺傜偣銆�

鏈€鏈夌敤鍜屾渶甯哥敤鐨剆top瑙勫垯鏄疭ELF銆両MMEDIATE_RESTARTt鎴朙OWER_PRIORITY_IMMEDIATE_RESTART銆�

涓嶈繃锛屽�傛灉浣犲�硅櫄骞诲紩鎿庣殑琛屼负鏍戝舰鎴愪簡鎯�鎬ф€濈淮锛屽氨瑕佸皬蹇冧簡銆傚湪NPBehave涓�锛孡OWER_PRIORITY鍜孊OTH鍏锋湁绋嶅井涓嶅悓鐨勫惈涔夈€侷MMEDIATE_RESTART瀹為檯涓婂尮閰峌nreal鐨凚oth锛岃€孡OWER_PRIORITY_IMMEDIATE_RESTART鍖归厤Unreal鐨凩ower Priority銆�

浣滆€呮彁渚涗簡濡備笅缁堟�㈠師鍒�

- Stops.NONE锛氳�呴グ鍣ㄥ彧浼氬湪鍚�鍔ㄦ椂妫€鏌ヤ竴娆″畠鐨勭姸鎬侊紝骞朵笖姘歌繙涓嶄細鍋滄��浠讳綍姝ｅ湪杩愯�岀殑鑺傜偣銆�
- Stops.SELF锛氳�呴グ鍣ㄥ皢鍦ㄥ惎鍔ㄦ椂妫€鏌ヤ竴娆″畠鐨勬潯浠剁姸鎬侊紝濡傛灉婊¤冻锛屽畠灏嗙户缁�瑙傚療榛戞澘鐨勫彉鍖栥€備竴鏃︿笉鍐嶆弧瓒宠�ユ潯浠讹紝瀹冨皢缁堟�㈣嚜韬�锛屽苟璁╃埗缁勫悎缁х画澶勭悊瀹冪殑涓嬩竴涓�鑺傜偣銆�
- Stops.LOWER_PRIORITY锛氳�呴グ鍣ㄥ皢鍦ㄥ惎鍔ㄦ椂妫€鏌ュ畠鐨勭姸鎬侊紝濡傛灉涓嶆弧瓒筹紝瀹冨皢瑙傚療榛戞澘鐨勫彉鍖栥€備竴鏃︽潯浠舵弧瓒筹紝瀹冨皢鍋滄�㈡瘮姝ょ粨鐐逛紭鍏堢骇杈冧綆鐨勮妭鐐癸紝鍏佽�哥埗缁勫悎缁х画澶勭悊涓嬩竴涓�鑺傜偣
- Stops.BOTH锛氳�呴グ鍣ㄥ皢鍚屾椂鍋滄��:self鍜屼紭鍏堢骇杈冧綆鐨勮妭鐐广€�
- Stops.LOWER_PRIORITY_IMMEDIATE_RESTART锛氫竴鏃﹀惎鍔�锛岃�呴グ鍣ㄥ皢妫€鏌ュ畠鐨勭姸鎬侊紝濡傛灉涓嶆弧瓒筹紝瀹冨皢瑙傚療榛戞澘鐨勫彉鍖栥€備竴鏃︽潯浠舵弧瓒筹紝瀹冨皢鍋滄��浼樺厛绾ц緝浣庣殑鑺傜偣锛屽苟鍛戒护鐖剁粍鍚堢珛鍗抽噸鍚�姝よ�呴グ鍣ㄣ€�
- Stops.IMMEDIATE_RESTART锛氫竴鏃﹀惎鍔�锛岃�呴グ鍣ㄥ皢妫€鏌ュ畠鐨勭姸鎬侊紝濡傛灉涓嶆弧瓒筹紝瀹冨皢瑙傚療榛戞澘鐨勫彉鍖栥€備竴鏃︽潯浠舵弧瓒筹紝瀹冨皢鍋滄��浼樺厛绾ц緝浣庣殑鑺傜偣锛屽苟鍛戒护鐖剁粍鍚堢珛鍗抽噸鍚�瑁呴グ鍣ㄣ€傛�ｅ�傚湪杩欎袱绉嶆儏鍐典笅锛屼竴鏃︿笉鍐嶆弧瓒虫潯浠讹紝瀹冧篃灏嗗仠姝㈣嚜宸便€�

### 榛戞澘鐨勬浛浠ｅ搧

鍦∟PBehave涓�锛屾偍鍦ㄤ竴涓狹onoBehaviour涓�瀹氫箟鎮ㄧ殑琛屼负鏍戯紝鍥犱负娌℃湁蹇呰�佸皢鎵€鏈夊唴瀹归兘瀛樺偍鍦ㄩ粦鏉夸腑銆傚�傛灉娌℃湁BlackboardDecorator鎴朆lackboardQuery锛屽垯浣跨敤鍏朵粬缁堟�㈣�勫垯鑰屼笉鏄疭tops.NONE銆備綘鍙�鑳芥牴鏈�涓嶉渶瑕佸畠浠�鍑虹幇鍦ㄩ粦鏉夸笂銆傛偍杩樺彲浠ヤ娇鐢ㄦ櫘閫氱殑鎴愬憳鍙橀噺鈥斺€斿畠閫氬父鏇村共鍑€銆佺紪鍐欓€熷害鏇村揩銆佹€ц兘鏇村ソ銆傝繖鎰忓懗鐫€鍦ㄨ繖绉嶆儏鍐典笅锛屾偍涓嶄細浣跨敤NPBehave鐨勪簨浠堕┍鍔ㄧ壒鎬э紝浣嗚繖閫氬父鏄�涓嶅繀瑕佺殑銆�

濡傛灉浣犳兂鍦ㄤ笉浣跨敤榛戞澘鐨勬儏鍐典笅浣跨敤stopsOnChange缁堟�㈣�勫垯锛孨PBehave涓�瀛樺湪涓ょ�嶆浛浠ｆ柟娉�:

1. 浣跨敤甯歌�勬潯浠惰�呴グ鍣ㄣ€傝繖涓�瑁呴グ鍣ㄦ湁涓€涓�鍙�閫夌殑stopsOnChange `缁堟�㈣�勫垯`鍙傛暟銆傚綋鎻愪緵闄�Stops.NONE涔嬪�栫殑浠讳綍鍏朵粬鍊硷紝涓旂粰瀹氭煡璇㈠嚱鏁扮殑缁撴灉鍙戠敓鏇存敼鏃讹紝鏉′欢灏嗛�戠箒鍦版�€鏌ユ潯浠跺苟鏍规嵁stop瑙勫垯涓�鏂�鑺傜偣銆傝�锋敞鎰忥紝姝ゆ柟娉曚笉鏄�浜嬩欢椹卞姩鐨勶紝瀹冩煡璇㈡瘡涓€甯�(鎴栧湪鎻愪緵鐨勬椂闂撮棿闅斿唴)锛屽洜姝ゅ�傛灉澶ч噺浣跨敤瀹冧滑锛屽彲鑳戒細瀵艰嚧澶ч噺涓嶅繀瑕佺殑鏌ヨ��銆傜劧鑰岋紝瀵逛簬绠€鍗曠殑鎯呭喌锛屽畠閫氬父鏄�瓒冲�熺殑锛屽苟涓旀瘮Blackboard-Key銆丼ervice鍜孊lackboardCondition鐨勭粍鍚堢畝鍗曞緱澶氥€�
2. 鏋勫缓鑷�宸辩殑浜嬩欢椹卞姩鐨勮�呴グ鍣ㄣ€傚疄闄呬笂闈炲父绠€鍗曪紝鍙�闇€浠嶰bservingDecorator鎵╁睍骞堕噸鍐檌sConditionMet()銆乻tartobservice()鍜宻topobservation()鏂规硶銆�

### 鑺傜偣鎵ц�岀粨鏋�
鍦∟PBehave涓�锛岃妭鐐瑰彲浠ユ垚鍔熶篃鍙�浠ュけ璐ャ€備笌浼犵粺鐨勮�屼负鏍戜笉鍚岋紝鑺傜偣鎵ц�屾椂娌℃湁杩斿洖缁撴灉銆傜浉鍙嶏紝涓€鏃﹁妭鐐规墽琛屽畬鎴愶紙鎴愬姛鎴栧け璐ワ級锛岃妭鐐规湰韬�灏嗗憡璇夌埗鑺傜偣銆傚湪鍒涘缓鑷�宸辩殑鑺傜偣绫诲瀷鏃讹紝鍔″繀璁颁綇杩欎竴鐐广€�

### 鑺傜偣绫诲瀷
鍦∟PBehave涓�锛屾垜浠�鏈夊洓绉嶄笉鍚岀殑鑺傜偣绫诲瀷:

1. 鏍硅妭鐐癸紙Root锛夛細鏍硅妭鐐瑰彧鏈変竴涓�瀛愯妭鐐瑰彲浠ュ惎鍔ㄦ垨鍋滄�㈡暣涓�琛屼负鏍戙€�
2. 缁勫悎鑺傜偣锛圕omposite锛夛細鏈夊�氫釜瀛愯妭鐐癸紝鐢ㄤ簬鎺у埗瀹冧滑鐨勫摢涓�瀛愯妭鐐硅��鎵ц�屻€傞『搴忓拰缁撴灉涔熸槸鐢辫繖绉嶈妭鐐瑰畾涔夌殑銆�
3. 瑁呴グ鑺傜偣锛圖ecorator锛夛細濮嬬粓`鍙�鏈変竴涓�瀛愯妭鐐筦锛岀敤浜庝慨鏀瑰瓙鑺傜偣鐨勭粨鏋滄垨鍦ㄦ墽琛屽瓙鑺傜偣鏃舵墽琛屽叾浠栨搷浣�(渚嬪�傦紝鏇存柊榛戞澘鐨凷ervice)
4. 浠诲姟鑺傜偣锛圱ask锛夛細杩欎簺鏄�鍋氬疄闄呭伐浣滅殑鏁翠釜琛屼负鏍戜腑鐨勬爲鍙躲€傛偍鏈€鏈夊彲鑳戒负瀹冧滑鍒涘缓鑷�瀹氫箟绫汇€傛偍鍙�浠ュ皢鎿嶄綔涓巐ambdas鎴栧嚱鏁颁竴璧蜂娇鐢ㄢ€斺€斿�逛簬鏇村�嶆潅鐨勪换鍔★紝鍒涘缓浠诲姟鐨勬柊瀛愮被閫氬父鏄�鏇村ソ鐨勯€夋嫨銆傚�傛灉浣犺繖鏍峰仛浜嗭紝涓€瀹氳�侀槄璇婚粍閲戣�勫垯銆�

### 缁堟�㈡爲
濡傛灉浣犵殑鎬�鐗╄��鏉€姝讳簡锛屾垨鑰呬綘閿€姣佷簡娓告垙瀵硅薄锛屼綘搴旇�ュ仠姝㈡爲銆備綘鍙�浠ュ湪浣犵殑鑴氭湰涓�鍔犲叆濡備笅鍐呭��:
```csharp
    // ...
    public void OnDestroy()
    {
        StopBehaviorTree();
    }

    public void StopBehaviorTree()
    {
        if ( behaviorTree != null && behaviorTree.CurrentState == Node.State.ACTIVE )
        {
            behaviorTree.Stop();
        }
    }
    // ...
```
### 杩愯�屾椂Debugger
鍙�浠ヤ娇鐢ㄨ皟璇曞櫒缁勪欢鍦ㄨ繍琛屾椂鍦ㄦ�€鏌ュ櫒涓�璋冭瘯琛屼负鏍戙€�
![NPBehave Debugger](https://github.com/meniku/NPBehave/blob/master/README-Debugger.png)

### 鍏变韩榛戞澘
鎮ㄥ彲浠ラ€夋嫨鍦ˋI鐨勫�氫釜瀹炰緥涔嬮棿鍏变韩榛戞澘銆傚�傛灉鎮ㄦ兂瀹炵幇鏌愮�嶉泦缇よ�屼负锛岃繖灏嗛潪甯告湁鐢ㄣ€傛�ゅ�栵紝鎮ㄥ彲浠ュ垱寤洪粦鏉垮眰娆＄粨鏋勶紝杩欏厑璁告偍灏嗗叡浜�榛戞澘涓庨潪鍏变韩榛戞澘缁勫悎璧锋潵銆�
鎮ㄥ彲浠ヤ娇鐢║nityContext.GetSharedBlackboard(name)鍦ㄤ换浣曞湴鏂硅�块棶鍏变韩鐨刡lackboard瀹炰緥銆�

### 鎷撳睍搴�
璇峰弬鑰冪幇鏈夌殑鑺傜偣瀹炵幇浜嗚В濡備綍鍒涘缓鑷�瀹氫箟鑺傜偣绫诲瀷锛屼絾鏄�鍦ㄥ垱寤轰箣鍓嶈嚦灏戣�侀槄璇讳互涓嬮粍閲戣�勫垯銆�

#### 榛勯噾娉曞垯

1. **姣忔�¤皟鐢―oStop()閮藉繀椤诲�艰嚧璋冪敤Stopped(result)**銆傝繖鏄�闈炲父閲嶈�佺殑!鎮ㄩ渶瑕佺‘淇濆湪DoStop()涓�璋冪敤浜哠topped()锛屽洜涓篘PBehave闇€瑕佽兘澶熷湪杩愯�屾椂绔嬪嵆鍙栨秷姝ｅ湪杩愯�岀殑鍒嗘敮銆傝繖涔熸剰鍛崇潃浣犳墍鏈夌殑瀛愯妭鐐逛篃灏嗚皟鐢⊿topped(),杩欏弽杩囨潵鍙堜娇寰楀畠寰堝�规槗缂栧啓鍙�闈犵殑decorator鐢氳嚦composite鑺傜偣:鍦―oStop()閲屼綘鍙�闇€瑕佽皟鐢╝ctive鐘舵€佷笅鐨勫�╁瓙Stop()鍑芥暟,浠栦滑灏嗚疆娴佹墽琛孋hildStopped()銆俙鏈€缁堜細鍥炴函鍒颁笂灞傝妭鐐圭殑Stopped()鍑芥暟锛乣璇锋煡鐪嬬幇鏈夌殑瀹炵幇浠ヤ緵鍙傝€冦€�
2. **Stopped()鏄�鎮ㄥ仛鐨勬渶鍚庝竴涓�璋冪敤**锛屽湪璋冪敤Stopped鍚庝笉瑕佷慨鏀逛换浣曠姸鎬佹垨璋冪敤浠讳綍涓滆タ銆傝繖鏄�鍥犱负Stopped灏嗙珛鍗崇户缁�閬嶅巻鍏朵粬鑺傜偣涓婄殑鏍戯紝濡傛灉涓嶈€冭檻杩欎竴鐐癸紝灏嗗畬鍏ㄧ牬鍧忚�屼负鏍戠殑鐘舵€併€�
3. **姣忎竴涓�娉ㄥ唽鐨勬椂閽熸垨榛戞澘瑙傚療鑰呮渶缁堥兘闇€瑕佸垹闄�**銆傚ぇ澶氭暟鏃跺€欎綘璋冪敤Stopped()涔嬪墠绔嬪埢娉ㄩ攢浣犵殑鍥炶皟鍑芥暟,涓嶈繃鍙�鑳戒細鏈変緥澶�,姣斿�侭lackboardCondition浣胯�傚療鑰呭�勪簬璀︽儠鐘舵€佺洿鍒扮埗缁勫悎缁撶偣缁堟��,瀹冮渶瑕佽兘澶熷�归粦鏉夸笂鍊兼敼鍙樺強鏃朵綔鍑哄弽搴旓紝鍗充娇鑺傜偣鏈�韬�骞朵笉娲昏穬銆�

### 瀹炵幇浠诲姟
瀵逛簬浠诲姟锛屽彲浠ヤ粠浠诲姟绫绘墿灞曞苟瑕嗙洊DoStart()鍜孌oStop()鏂规硶銆傚湪DoStart()涓�锛屾偍鍚�鍔ㄦ偍鐨勯€昏緫锛屼竴鏃︽偍瀹屾垚浜嗭紝鎮ㄥ皢浣跨敤閫傚綋鐨勭粨鏋滆皟鐢⊿topped(bool result)銆傛偍鐨勮妭鐐瑰彲鑳借��鍙︿竴涓�鑺傜偣鍙栨秷锛屽洜姝よ�风‘淇濆疄鐜癉oStop()锛岃繘琛岄€傚綋鐨勬竻鐞嗗苟鍦ㄥ畠涔嬪悗绔嬪嵆璋冪敤Stopped(bool result)銆�
瀵逛簬涓€涓�鐩稿�圭畝鍗曠殑绀轰緥锛岃�锋煡鐪媁ait Task.cs銆�
姝ｅ�傞粍閲戣�勫垯閮ㄥ垎宸茬粡鎻愬埌鐨勶紝鍦∟PBehave涓�锛屾偍蹇呴』鍦ㄨ妭鐐瑰仠姝�涔嬪悗濮嬬粓璋冪敤Stopped(bool result)銆傚洜姝わ紝鐩�鍓嶄笉鏀�鎸佸湪澶氫釜甯т笂鎸傝捣鍙栨秷鎿嶄綔锛岃繖灏嗗�艰嚧涓嶅彲棰勬祴鐨勮�屼负銆�

### 瀹炵幇瑙傚療瑁呴グ鍣�
缂栧啓瑁呴グ鍣ㄨ�佹瘮缂栧啓浠诲姟澶嶆潅寰楀�氥€傜劧鑰岋紝涓轰簡鏂逛究璧疯�侊紝瀛樺湪涓€涓�鐗规畩鐨勫熀绫汇€侽bservingDecorator銆傝繖涓�绫诲彲鐢ㄤ簬绠€鍗曞湴瀹炵幇鈥滄潯浠垛€濊�呴グ鍣�锛岃繖浜涜�呴グ鍣ㄥ彲閫夊湴浣跨敤stopsOnChange 缁堟�㈣�勫垯銆�
鎮ㄦ墍瑕佸仛鐨勫氨鏄�浠庡畠ObservingDecorator鎵╁睍骞惰�嗙洊bool IsConditionMet()鏂规硶銆傚�傛灉甯屾湜鏀�鎸乻top - rules锛岃繕蹇呴』瀹炵幇startobservice()鍜宻topobserve()銆傚�逛簬涓€涓�绠€鍗曠殑绀轰緥锛岃�锋煡鐪婥ondition Decorator.cs

### 瀹炵幇甯歌�勮�呴グ鍣�
瀵逛簬甯歌�勮�呴グ鍣�锛屽彲浠ヤ粠Decorator.cs鎵╁睍骞惰�嗙洊DoStart()銆丏oStop()鍜孌oChildStopped(Node child, bool result)鏂规硶銆�
鎮ㄥ彲浠ラ€氳繃璁块棶Decoratee灞炴€у惎鍔ㄦ垨鍋滄�㈠凡瑁呴グ鑺傜偣锛屽苟鍦ㄥ叾涓婅皟鐢╯tart()鎴杝top()銆�
濡傛灉鎮ㄧ殑decorator鎺ユ敹鍒癉oStop()璋冪敤锛屽畠灏嗚礋璐ｇ浉搴斿湴鍋滄��Decoratee锛屽苟涓斿湪杩欑�嶆儏鍐典笅涓嶄細绔嬪嵆璋冪敤Stopped(bool result)銆傜浉鍙嶏紝瀹冨皢鍦―oChildStopped(Node child, bool result)鏂规硶涓�鎵ц�岃�ユ搷浣溿€傝�锋敞鎰忥紝DoChildStopped(Node child, bool result)骞朵笉涓€瀹氭剰鍛崇潃鎮ㄧ殑decorator鍋滄��浜哾ecoratee, decoratee鏈�韬�涔熷彲鑳藉仠姝�锛屽湪杩欑�嶆儏鍐典笅锛屾偍涓嶉渶瑕佺珛鍗冲仠姝�decoratee(濡傛灉鎮ㄦ兂瀹炵幇璇稿�傚喎鍗寸瓑鍔熻兘锛岃繖鍙�鑳藉緢鏈夌敤)銆傝�佹煡鏄庤�呴グ鍣ㄦ槸鍚﹁��鍋滄��锛屽彲浠ユ煡璇㈠畠鐨刬sstoprequired灞炴€с€�
瀵逛簬闈炲父鍩烘湰鐨勫疄鐜帮紝璇锋煡鐪婩ailer Node.cs;瀵逛簬绋嶅井澶嶆潅涓€鐐圭殑瀹炵幇锛岃�锋煡鐪婻epeater Node.cs銆�
姝ゅ�栵紝鎮ㄨ繕鍙�浠ュ疄鐜癉oParentCompositeStopped()鏂规硶锛屽嵆浣挎偍鐨勮�呴グ鍣ㄥ�勪簬闈炴椿鍔ㄧ姸鎬侊紝涔熷彲浠ヨ皟鐢ㄨ�ユ柟娉曘€傚�傛灉鎮ㄦ兂涓哄湪瑁呴グ鍣╯topped鍚庝粛淇濇寔娲诲姩鐨勪睛鍚�鍣ㄦ墽琛岄�濆�栫殑娓呯悊宸ヤ綔锛岃繖鏄�闈炲父鏈夌敤鐨勩€備互ObservingDecorator涓轰緥銆�

### 瀹炵幇缁勫悎
缁勫悎鑺傜偣闇€瑕佸�瑰簱鏈夋洿娣卞叆鐨勭悊瑙ｏ紝閫氬父涓嶉渶瑕佸疄鐜版柊鐨勮妭鐐广€傚�傛灉鎮ㄧ湡鐨勯渶瑕佷竴涓�鏂扮殑缁勫悎锛岃�峰湪GitHub椤圭洰涓婂垱寤轰竴涓�绁ㄦ嵁锛屾垨鑰呬笌鎴戣仈绯伙紝鎴戝皢灏藉姏甯�鍔╂偍姝ｇ‘鍦板畬鎴愬畠銆�

### 缁撶偣鐘舵€�
寰堟湁鍙�鑳戒綘涓嶉渶瑕佽�块棶瀹冧滑锛屼絾浜嗚В瀹冧滑浠嶇劧鏄�浠跺ソ浜�:

- ACTIVE:鑺傜偣宸插惎鍔�锛屼絾灏氭湭鍋滄��銆�
- STOP_REQUESTED:鑺傜偣褰撳墠姝ｅ湪鍋滄��锛屼絾灏氭湭璋冪敤Stopped()鏉ラ€氱煡鐖惰妭鐐广€�
- INACTIVE:鑺傜偣宸插仠姝�銆�

鍙�浠ヤ娇鐢–urrentState灞炴€ф�€绱㈠綋鍓嶇姸鎬�

### 鏃堕挓
鎮ㄥ彲浠ヤ娇鐢ㄨ妭鐐逛腑鐨勬椂閽熸敞鍐岃�℃椂鍣�锛屾垨鑰呭湪姣忎竴甯т笂寰楀埌閫氱煡銆備娇鐢≧ootNode.Clock璁块棶鏃堕挓銆傛煡鐪媊Wait Task.cs`浠ヨ幏寰楀叧浜庡�備綍鍦ㄦ椂閽熶笂娉ㄥ唽璁℃椂鍣ㄧ殑绀轰緥銆�
榛樿�ゆ儏鍐典笅锛岃�屼负鏍戝皢浣跨敤UnityContext鎸囧畾鐨勫叏灞€鏃堕挓銆傝繖涓�鏃堕挓姣忎竴甯ч兘鏇存柊涓€娆°€傚湪鏌愪簺鎯呭喌涓嬶紝浣犲彲鑳芥兂瑕佹嫢鏈夋洿澶氱殑鎺у埗鏉冦€備緥濡傦紝鎮ㄥ彲鑳芥兂瑕侀檺鍒舵垨鏆傚仠瀵逛竴缁凙I鐨勬洿鏂般€傜敱浜庤繖涓�鍘熷洜锛屾偍鍙�浠ュ悜鏍硅妭鐐瑰拰Blackboard鎻愪緵鑷�宸辩殑鍙楁帶鏃堕挓瀹炰緥锛岃繖鍏佽�告偍绮剧‘鍦版帶鍒朵綍鏃舵洿鏂拌�屼负鏍戙€傛煡鐪� Clock Throttling .cs銆�

## 缁撶偣绫诲瀷姹囨€�
### Root

- Root(Node mainNode):鏃犱紤姝㈠湴杩愯�宮ainNode锛屼笉璁轰换浣曟儏鍐�
- Root(Blackboard Blackboard, Node mainNode):浣跨敤缁欏畾鐨勯粦鏉匡紝鑰屼笉鏄�瀹炰緥鍖栦竴涓�;鏃犱紤姝㈠湴杩愯�岀粰瀹氱殑mainNode锛屼笉璁轰换浣曟儏鍐�
- Root(Blackboard blackboard, Clock clock, Node mainNode):浣跨敤缁欏畾鐨勯粦鏉胯€屼笉鏄�瀹炰緥鍖栦竴涓�;浣跨敤缁欏畾鐨勬椂閽燂紝鑰屼笉鏄�浣跨敤UnityContext涓�鐨勫叏灞€鏃堕挓;鏃犱紤姝㈠湴杩愯�岀粰瀹氱殑mainNode锛屼笉璁轰换浣曟儏鍐�

### 缁勫悎缁撶偣
#### Selector
- Selector(params Node[] children):鎸夐『搴忚繍琛屽瓙鍏冪礌锛岀洿鍒板叾涓�涓€涓�瀛愬厓绱犳垚鍔�(濡傛灉鍏朵腑涓€涓�瀛愬厓绱犳垚鍔燂紝鍒欐垚鍔�)銆�
#### Sequence
- Sequence(params Node[] children):鎸夐『搴忚繍琛屽瓙鑺傜偣锛岀洿鍒板叾涓�涓€涓�澶辫触(濡傛灉鎵€鏈夊瓙鑺傜偣閮芥病鏈夊け璐ワ紝鍒欐垚鍔�)銆�
#### Parallel
- Parallel(Policy successPolicy, Policy failurePolicy, params Node[] children): 骞惰�岃繍琛屽瓙鑺傜偣銆�
- 褰揻ailurePolocity涓篜olociy.ONE銆傚綋鍏朵腑涓€涓�瀛╁瓙澶辫触鏃讹紝骞惰�屽氨浼氬仠姝�锛岃繑鍥炲け璐ャ€�
- 褰搒uccessPolicy涓篜olicy.ONE銆傚綋鍏朵腑涓€涓�瀛╁瓙澶辫触鏃讹紝骞惰�屽皢鍋滄��锛岃繑鍥炴垚鍔熴€�
- 濡傛灉骞惰�屾病鏈夊洜涓篜olicy.ONE鑰屽仠姝�銆傚畠浼氫竴鐩存墽琛岋紝鐩村埌鎵€鏈夌殑瀛愯妭鐐归兘瀹屾垚锛岀劧鍚庡�傛灉鎵€鏈夌殑瀛愯妭鐐归兘鎴愬姛鎴栬€呭け璐ワ紝瀹冨氨浼氳繑鍥炴垚鍔熴€�
#### RandomSelector
- RandomSelector(params Node[] children):鎸夐殢鏈洪『搴忚繍琛屽瓙杩涚▼锛岀洿鍒板叾涓�涓€涓�瀛愯繘绋嬫垚鍔�(濡傛灉鍏朵腑涓€涓�瀛愯繘绋嬫垚鍔燂紝鍒欐垚鍔�)銆傛敞鎰忥紝瀵逛簬鎵撴柇瑙勫垯锛屾渶鍒濈殑椤哄簭瀹氫箟浜嗕紭鍏堢骇銆�
#### RandomSequence
- RandomSequence(params Node[] children):浠ラ殢鏈洪『搴忚繍琛屽瓙鑺傜偣锛岀洿鍒板叾涓�涓€涓�澶辫触(濡傛灉娌℃湁瀛愯妭鐐瑰け璐ワ紝鍒欐垚鍔�)銆傛敞鎰忥紝瀵逛簬鎵撴柇瑙勫垯锛屾渶鍒濈殑椤哄簭瀹氫箟浜嗕紭鍏堢骇銆�
### 浠诲姟缁撶偣
#### Action

- Action(System.Action action):(鎬绘槸绔嬪嵆鎴愬姛瀹屾垚)
- Action(System.Func<bool> singleFrameFunc): 鍙�浠ユ垚鍔熸垨澶辫触鐨勬搷浣�(杩斿洖false to fail)
- Action(Func<bool, Result> multiframeFunc):鍙�浠ュ湪澶氫釜甯т笂鎵ц�岀殑鎿嶄綔(
Result.BLOCKED鈥斺€斾綘鐨勮�屽姩杩樻病鏈夊噯澶囧ソ
Result.PROGRESS鈥斺€斿綋浣犲繖鐫€杩欎釜琛屼负鐨勬椂鍊欙紝
Result.SUCCESS鎴朢esult.FAILED鈥斺€旀垚鍔熸垨澶辫触)銆�
- Action(Func<Request, Result> multiframeFunc2): 涓庝笂闈㈢被浼硷紝浣嗘槸Request浼氱粰浣犱竴涓�鐘舵€佷俊鎭�:
Request.START琛ㄧず瀹冩槸鎮ㄧ殑鎿嶄綔鎴栬繑鍥炵粨鏋滅殑绗�涓€涓�鏍囪�版垨鑰呮槸Result.BLOCKED鏈€鍚庝竴涓�鏍囪�般€�
Request.UPDATE琛ㄧず鎮ㄦ渶鍚庝竴娆¤繑鍥濺equest.PROGRESS;
Request.CANCEL鎰忓懗鐫€鎮ㄩ渶瑕佸彇娑堟搷浣滃苟杩斿洖缁撴灉銆傛垚鍔熸垨鑰匯esult.FAILED銆�

#### Wait

- Wait(float seconds): 绛夊緟缁欏畾鐨勭�掞紝闅忔満璇�宸�涓�0.05 *绉�
- Wait(float seconds, float randomVariance): 鐢ㄧ粰瀹氱殑闅忔満鍙橀噺绛夊緟缁欏畾鐨勭�掓暟
- Wait(string blackboardKey, float randomVariance = 0f): 
- Wait(System.Func<float> function, float randomVariance = 0f): 绛夊緟鍦ㄧ粰瀹氱殑blackboardKey涓�璁剧疆涓篺loat鐨勭�掓暟
	
#### WaitUntilStopped

- WaitUntilStopped(bool sucessWhenStopped = false):绛夊緟琚�鍏朵粬鑺傜偣鍋滄��銆傚畠閫氬父鐢ㄤ簬Selector鐨勬湯灏撅紝绛夊緟浠讳綍before澶寸殑鍚岀骇BlackboardCondition銆丅lackboardQuery鎴朇ondition鍙樹负娲诲姩鐘舵€併€�

### 瑁呴グ鍣ㄧ粨鐐�
#### BlackboardCondition
- BlackboardCondition(string key, Operator operator, object value, Stops stopsOnChange, Node decoratee): 鍙�鏈夊綋榛戞澘鐨勯敭鍖归厤op / value鏉′欢鏃讹紝鎵嶆墽琛宒ecoratee鑺傜偣銆傚�傛灉stopsOnChange涓嶆槸NONE锛屽垯鑺傜偣灏嗘牴鎹畇topsOnChange stop瑙勫垯瑙傚療榛戞澘涓婄殑鍙樺寲骞跺仠姝㈣繍琛岃妭鐐圭殑鎵ц�屻€�
- BlackboardCondition(string key, Operator operator, Stops stopsOnChange, Node decoratee): 鍙�鏈夊綋榛戞澘鐨勯敭涓巓p鏉′欢鍖归厤鏃舵墠鎵ц�宒ecoratee鑺傜偣(渚嬪�傦紝瀵逛簬涓€涓�鍙�妫€鏌�IS_SET鐨勬搷浣滄暟鎿嶄綔绗�)銆傚�傛灉stopsOnChange涓嶆槸NONE锛屽垯鑺傜偣灏嗘牴鎹畇topsOnChange stop瑙勫垯瑙傚療榛戞澘涓婄殑鍙樺寲骞跺仠姝㈣繍琛岃妭鐐圭殑鎵ц�屻€�
#### BlackboardQuery
- BlackboardQuery(string[] keys, Stops stopsOnChange, System.Func<bool> query, Node decoratee):BlackboardCondition鍙�鍏佽�告�€鏌ヤ竴涓�閿�锛岃€岃繖涓�灏嗚�傚療澶氫釜榛戞澘閿�锛屽苟鍦ㄥ叾涓�涓€涓�鍊煎彂鐢熷彉鍖栨椂绔嬪嵆璁＄畻缁欏畾鐨勬煡璇㈠嚱鏁帮紝浠庤€屽厑璁告偍鍦ㄩ粦鏉夸笂鎵ц�屼换鎰忔煡璇�銆傚畠灏嗘牴鎹畇topsOnChange stop瑙勫垯鍋滄�㈣繍琛岃妭鐐广€�
#### Condition
- Condition(Func<bool> condition, Node decoratee): 濡傛灉缁欏畾鏉′欢杩斿洖true锛屽垯鎵ц�宒ecoratee鑺傜偣
- Condition(Func<bool> condition, Stops stopsOnChange, Node decoratee): 濡傛灉缁欏畾鏉′欢杩斿洖true锛屽垯鎵ц�宒ecoratee鑺傜偣銆傛牴鎹畇topsOnChange stop瑙勫垯閲嶆柊璇勪及姣忎釜甯х殑鏉′欢骞跺仠姝㈣繍琛岃妭鐐广€�
- Condition(Func<bool> condition, Stops stopsOnChange, float checkInterval, float randomVariance, Node decoratee): 濡傛灉缁欏畾鏉′欢杩斿洖true锛屽垯鎵ц�宒ecoratee鑺傜偣銆傚湪缁欏畾鐨勬牎楠岄棿闅斿拰闅忔満鏂瑰樊澶勯噸鏂拌瘎浼版潯浠讹紝骞舵牴鎹畇topsOnChange stop瑙勫垯鍋滄�㈣繍琛岃妭鐐广€�
#### Cooldown
- Cooldown(float cooldownTime, Node decoratee):绔嬪嵆杩愯�宒ecoratee锛屼絾鍓嶆彁鏄�鏈€鍚庝竴娆℃墽琛岃嚦灏戞病鏈夎秴杩嘽ooldownTime
- Cooldown(float cooldownTime, float randomVariation, Node decoratee): 绔嬪嵆杩愯�宒ecoratee锛屼絾鍓嶆彁鏄�鏈€鍚庝竴娆℃墽琛岃嚦灏戞病鏈夎秴杩囦娇鐢╮andomVariation杩涜�岀殑cooldownTime
- Cooldown(float cooldownTime, bool startAfterDecoratee, bool resetOnFailiure, Node decoratee):  绔嬪嵆杩愯�宒ecoratee锛屼絾鍓嶆彁鏄�鏈€鍚庝竴娆℃墽琛岃嚦灏戞病鏈夎秴杩囦娇鐢╮andomVariation杩涜�岀殑cooldownTime锛屽綋resetOnFailure涓虹湡鏃讹紝濡傛灉淇�楗拌妭鐐瑰け璐ワ紝鍒欓噸缃�鍐峰嵈鏃堕棿
- Cooldown(float cooldownTime, float randomVariation, bool startAfterDecoratee, bool resetOnFailiure, Node decoratee)  绔嬪嵆杩愯�宒ecoratee锛屼絾鍓嶆彁鏄�鏈€鍚庝竴娆℃墽琛岃嚦灏戞病鏈夎秴杩囦娇鐢╮andomVariation杩涜�岀殑cooldownTime锛屽綋startAfterDecoratee涓簍rue鏃讹紝灏嗗湪decoratee瀹屾垚鍚庤€屼笉鏄�鍚�鍔ㄦ椂鍚�鍔ㄥ喎鍗磋�℃椂鍣ㄣ€傚綋resetOnFailure涓虹湡鏃讹紝濡傛灉淇�楗拌妭鐐瑰け璐ワ紝鍒欓噸缃�鍐峰嵈鏃堕棿銆�
#### Failer
- Failer(Node decoratee): 鎬绘槸澶辫触锛屼笉绠¤�呴グ鑰呯殑缁撴灉濡備綍銆�
#### Inverter
- Inverter(Node decoratee): 濡傛灉decoratee鎴愬姛锛屽垯閫嗗彉鍣ㄥけ璐�;濡傛灉decoratee澶辫触锛屽垯閫嗗彉鍣ㄦ垚鍔熴€�
#### Observer
- Observer(Action onStart, Action<bool> onStop, Node decoratee): 涓€鏃�decoratee鍚�鍔�锛岃繍琛岀粰瀹氱殑onStart lambda;涓€鏃�decoratee缁撴潫锛岃繍琛宱nStop(bool result) lambda銆傚畠鏈夌偣鍍忎竴绉嶇壒娈婄殑鏈嶅姟锛屽洜涓哄畠涓嶄細鐩存帴骞叉壈decoratee鐨勬墽琛屻€�
#### Random
- Random(float probability, Node decoratee): 浠ョ粰瀹氱殑姒傜巼锛�0鍒�1杩愯�宒ecoratee銆�
#### Repeater
- Repeater(Node decoratee): 鏃犻檺閲嶅�嶇粰瀹氱殑瑁呴グ锛岄櫎闈炲け璐�
- Repeater(int loopCount, Node decoratee): 鎵ц�岀粰瀹氱殑decoratee寰�鐜�娆℃暟(0琛ㄧずdecoratee姘歌繙涓嶄細杩愯��)銆傚�傛灉decoratee鍋滄��锛屽惊鐜�灏嗕腑姝�锛屽苟涓斾腑缁у櫒澶辫触銆傚�傛灉decoratee鐨勬墍鏈夋墽琛岄兘鎴愬姛锛岄偅涔堜腑缁у櫒灏嗕細鎴愬姛銆�
#### Service
- Service(Action service, Node decoratee): 杩愯�岀粰瀹氱殑鏈嶅姟鍑芥暟锛屽惎鍔╠ecoratee锛岀劧鍚庢瘡娆¤繍琛屾湇鍔°€�
- Service(float interval, Action service, Node decoratee): 杩愯�岀粰瀹氱殑鏈嶅姟鍑芥暟锛屽惎鍔╠ecoratee锛岀劧鍚庢寜缁欏畾鐨勯棿闅旇繍琛屾湇鍔°€�
- Service(float interval, float randomVariation, Action service, Node decoratee): 杩愯�岀粰瀹氱殑鏈嶅姟鍑芥暟锛屽惎鍔╠ecoratee锛岀劧鍚庡湪缁欏畾鐨勬椂闂撮棿闅斿唴浠ラ殢鏈哄彉閲忕殑鏂瑰紡杩愯�屾湇鍔°€�
#### Succeeder
- Succeeder(Node decoratee): 姘歌繙瑕佹垚鍔燂紝涓嶇�¤�呴グ鍣ㄦ槸鍚︽垚鍔�
#### TimeMax
- TimeMax(float limit, bool waitForChildButFailOnLimitReached, Node decoratee): 杩愯�岀粰瀹氱殑decoratee銆傚�傛灉decoratee娌℃湁鍦ㄩ檺鍒舵椂闂村唴瀹屾垚锛屽垯鎵ц�屽皢澶辫触銆傚�傛灉waitforchildbutfailonlimitarrived涓簍rue锛屽畠灏嗙瓑寰卍ecoratee瀹屾垚锛屼絾浠嶇劧澶辫触銆�
- TimeMax(float limit, float randomVariation, bool waitForChildButFailOnLimitReached, Node decoratee):杩愯�岀粰瀹氱殑decoratee銆傚�傛灉decoratee娌℃湁鍦ㄩ檺鍒跺拰闅忔満鍙樺寲鑼冨洿鍐呭畬鎴愶紝鍒欐墽琛屽皢澶辫触銆傚�傛灉waitforchildbutfailonlimitarrived涓簍rue锛屽畠灏嗙瓑寰卍ecoratee瀹屾垚锛屼絾浠嶇劧澶辫触銆�
#### TimeMin
- TimeMin(float limit, Node decoratee): 杩愯�岀粰瀹氱殑decoratee銆傚�傛灉decoratee鍦ㄨ揪鍒伴檺鍒舵椂闂翠箣鍓嶆垚鍔熷畬鎴愶紝decorator灏嗙瓑寰呯洿鍒拌揪鍒伴檺鍒讹紝鐒跺悗鏍规嵁decoratee鐨勭粨鏋滃仠姝㈡墽琛屻€傚�傛灉琚�瑁呴グ鑰呭湪杈惧埌闄愬埗鏃堕棿涔嬪墠澶辫触锛岃�呴グ鑰呭皢绔嬪嵆鍋滄��銆�
- TimeMin(float limit, bool waitOnFailure, Node decoratee): 杩愯�岀粰瀹氱殑decoratee銆傚�傛灉decoratee鍦ㄨ揪鍒伴檺鍒舵椂闂翠箣鍓嶆垚鍔熷畬鎴愶紝decorator灏嗙瓑寰呯洿鍒拌揪鍒伴檺鍒讹紝鐒跺悗鏍规嵁decoratee鐨勭粨鏋滃仠姝㈡墽琛屻€傚�傛灉waitOnFailure涓虹湡锛岄偅涔堝綋decoratee澶辫触鏃讹紝decoratee涔熷皢绛夊緟銆�
- TimeMin(float limit, float randomVariation, bool waitOnFailure, Node decoratee): 杩愯�岀粰瀹氱殑decoratee銆傚�傛灉decoratee鍦ㄨ揪鍒伴殢鏈哄彉鍖栨椂闂撮檺鍒朵箣鍓嶆垚鍔熷畬鎴愶紝decorator灏嗙瓑寰呯洿鍒拌揪鍒伴檺鍒讹紝鐒跺悗鏍规嵁decoratee鐨勭粨鏋滃仠姝㈡墽琛屻€傚�傛灉waitOnFailure涓虹湡锛岄偅涔堝綋decoratee澶辫触鏃讹紝decoratee涔熷皢绛夊緟銆�
#### WaitForCondition
- WaitForCondition(Func<bool> condition, Node decoratee): 寤惰繜decoratee鑺傜偣鐨勬墽琛岋紝鐩村埌鏉′欢涓虹湡锛屾�€鏌ユ瘡涓€甯�
- WaitForCondition(Func<bool> condition, float checkInterval, float randomVariance, Node decoratee): 寤惰繜decoratee鑺傜偣鐨勬墽琛岋紝鐩村埌鏉′欢涓虹湡锛屼娇鐢ㄧ粰瀹氱殑checkInterval鍜宺andomVariance杩涜�屾�€鏌�

## 鍚庤��
鏈�鏂囨。浠呬緵鍙傝€冿紝涓€鍒囦互浠ｇ爜涓哄噯锛�


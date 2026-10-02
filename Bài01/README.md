Bài Kiểm Tra số 1.
Câu 1: Trình bày sự khác nhau giữa Value Types (Kiểu giá trị) và Reference Types (Kiểu tham chiếu) trong C# về cơ chế lưu trữ vùng nhớ (Stack vs Heap).
Trong C#, kiểu dữ liệu được chia thành hai nhóm chính là Value Type và Reference Type.
1. Value Type – Kiểu giá trị
Value Type là kiểu dữ liệu mà biến lưu trực tiếp giá trị của dữ liệu. Khi một biến Value Type được gán cho một biến khác thì giá trị được sao chép sang biến mới. Hai biến sau đó hoạt động độc lập với nhau.
Các kiểu Value Type phổ biến gồm:
•	int
•	float
•	double
•	decimal
•	char
•	bool
•	struct
•	enum
2. Reference Type – Kiểu tham chiếu
Reference Type là kiểu dữ liệu mà biến lưu một tham chiếu đến đối tượng trong bộ nhớ. Đối tượng được tạo ra thường nằm trên Heap, còn biến tham chiếu giữ thông tin để truy cập đến đối tượng đó.
Các Reference Type phổ biến gồm:
•	class
•	object
•	string
•	array
•	interface
•	delegate
3. So sánh Stack và Heap
Stack là vùng nhớ thường được sử dụng cho các biến cục bộ, thông tin của lời gọi phương thức và một số dữ liệu có thời gian sống gắn với phạm vi thực thi.
Heap là vùng nhớ được sử dụng để cấp phát các đối tượng động. Các đối tượng được tạo bằng new thường được cấp phát trên managed heap và được .NET Garbage Collector quản lý.
Có thể hình dung:
Stack                    Heap
+---------+              +----------------+
| a = 10  |              | Student Object |
+---------+              | Name = "Nam"   |
| b = 20  |              +----------------+
+---------+                    ↑
                               |
                         s1, s2 tham chiếu
Tuy nhiên, cần lưu ý rằng cách nói "Value Type nằm trên Stack, Reference Type nằm trên Heap" chỉ là cách giải thích đơn giản. Trong thực tế, vị trí lưu trữ còn phụ thuộc vào ngữ cảnh. Ví dụ, một Value Type có thể là thành viên của một object và do đó nằm bên trong vùng nhớ của object trên Heap.
Kết luận
Value Type lưu trực tiếp giá trị và khi gán sẽ sao chép giá trị. Reference Type lưu tham chiếu đến đối tượng và khi gán sẽ sao chép tham chiếu.
Value Type → giá trị → sao chép giá trị.
Reference Type → tham chiếu → sao chép tham chiếu.
________________________________________
Câu 2: Tính năng Init-only Properties (init) trong C# 9/10 khác gì so với thuộc tính có set thông thường? Nêu trường hợp sử dụng thực tế.
Trong C#, Property được sử dụng để kiểm soát việc truy cập và thay đổi dữ liệu của đối tượng. Hai cách khai báo phổ biến là sử dụng set và init.
1. Property sử dụng set
Khi Property có set, thuộc tính có thể được gán giá trị và thay đổi sau khi đối tượng đã được khởi tạo.
Ví dụ:
class Student
{
    public string Name { get; set; }
}
Có thể sử dụng:
Student sv = new Student();

sv.Name = "Hanh";
sv.Name = "Nam";
Giá trị Name có thể thay đổi nhiều lần trong suốt vòng đời của đối tượng.
2. Init-only Property
init được giới thiệu từ C# 9. Nó cho phép thuộc tính được gán giá trị trong quá trình khởi tạo đối tượng nhưng không cho phép thay đổi sau khi quá trình khởi tạo hoàn tất.
3. So sánh
set	init
Có thể gán khi khởi tạo	Có thể gán khi khởi tạo
Có thể thay đổi sau khi khởi tạo	Không thể thay đổi sau khi khởi tạo
Dữ liệu có thể thay đổi	Dữ liệu có tính chất gần như chỉ đọc sau khởi tạo
Phù hợp dữ liệu thay đổi	Phù hợp dữ liệu cần cố định
4. Trường hợp sử dụng thực tế
init phù hợp với những thông tin cần được xác định khi tạo đối tượng nhưng không nên thay đổi sau đó.
Kết luận
set cho phép thay đổi Property sau khi đối tượng được tạo.
init chỉ cho phép thiết lập Property trong quá trình khởi tạo, giúp bảo vệ những dữ liệu cần cố định.

Câu 3: Phân biệt sự khác nhau giữa phương thức virtual ở lớp cha và phương thức override ở lớp con khi triển khai tính Đa hình (Polymorphism).
Đa hình (Polymorphism) là một trong bốn đặc tính quan trọng của lập trình hướng đối tượng. Trong C#, virtual và override thường được sử dụng để triển khai đa hình động.
1. virtual
virtual được sử dụng để khai báo một phương thức trong lớp cha, cho phép lớp con có thể ghi đè cách thực hiện phương thức đó.
2. override
override được sử dụng trong lớp con để ghi đè phương thức virtual hoặc abstract của lớp cha.
3. Thể hiện tính đa hình
Ta có:
Animal animal = new Dog();

animal.Sound();
Mặc dù biến animal có kiểu Animal, nhưng đối tượng thực tế được tạo ra là Dog.
Kết quả:
Gâu gâu
Điều này thể hiện đa hình động: phương thức được lựa chọn dựa trên kiểu thực tế của đối tượng tại thời điểm chạy chương trình.
4. So sánh
Virtual	override
Khai báo ở lớp cha	Khai báo ở lớp con
Cho phép phương thức được ghi đè	Thực hiện việc ghi đè
Là phương thức cơ sở	Là phiên bản triển khai mới
Dùng để hỗ trợ đa hình	Dùng để triển khai đa hình
Kết luận
virtual → lớp cha cho phép ghi đè.
override → lớp con ghi đè phương thức của lớp cha.
Hai từ khóa kết hợp với nhau giúp C# thực hiện đa hình động.


Câu 4: Tại sao thành phần static trong Class không thể truy xuất thông qua Object Instance?
Trong C#, từ khóa static dùng để khai báo một thành phần thuộc về lớp (Class) chứ không thuộc về từng đối tượng (Object) được tạo ra từ lớp đó.
Khi một thành phần được khai báo là static, nó chỉ có một bản duy nhất và được dùng chung cho tất cả các đối tượng thuộc lớp đó. Vì vậy, thành phần static
Kết luận: Thành phần static không thuộc về một Object cụ thể mà thuộc về Class, chỉ có một bản dùng chung cho toàn bộ Class. Vì vậy, nó được truy xuất bằng tên Class, còn thành phần không static được truy xuất thông qua Object Instance.
được truy xuất thông qua tên lớp, không phải thông qua Object Instance.


# BÀI 1 - LÝ THUYẾT C#

## Câu 1. Value Types và Reference Types

**Value Types** lưu trực tiếp giá trị. Các biến cục bộ thường được cấp phát trên Stack; khi truyền biến kiểu giá trị, giá trị được sao chép. Ví dụ: `int`, `double`, `decimal`, `bool`, `struct`, `enum`.

**Reference Types** lưu một tham chiếu đến đối tượng. Đối tượng được cấp phát trên Heap; biến tham chiếu giữ địa chỉ/tham chiếu đến đối tượng. Ví dụ: `class`, `string`, `array`, `object`.

Lưu ý: cách nói Stack/Heap là cách giải thích cơ bản; vị trí thực tế còn phụ thuộc JIT/runtime và ngữ cảnh. Điểm quan trọng là Value Type biểu diễn giá trị, còn Reference Type biến giữ tham chiếu tới đối tượng.

## Câu 2. init-only property và set

`set` cho phép gán thuộc tính sau khi đối tượng đã được tạo.

`init` (C# 9 trở lên) chỉ cho phép gán thuộc tính trong object initializer hoặc constructor; sau khi khởi tạo xong thì không thể gán lại từ bên ngoài.

Ví dụ:

```csharp
public class Student
{
    public string MaSV { get; init; } = "";
    public string HoTen { get; set; } = "";
}

var sv = new Student { MaSV = "SV01", HoTen = "An" };
sv.HoTen = "Bình";
// sv.MaSV = "SV02"; // lỗi vì MaSV là init-only
```

`init` phù hợp với dữ liệu cần cố định sau khi tạo đối tượng, giúp hạn chế thay đổi ngoài ý muốn.

## Câu 3. virtual và override

`virtual` được khai báo ở lớp cha để cho phép lớp con thay đổi cách thực hiện phương thức.

`override` được khai báo ở lớp con để cung cấp cách thực hiện mới cho phương thức `virtual` hoặc `abstract` của lớp cha.

Ví dụ:

```csharp
class Animal
{
    public virtual void Speak()
    {
        Console.WriteLine("Animal");
    }
}

class Dog : Animal
{
    public override void Speak()
    {
        Console.WriteLine("Gâu gâu");
    }
}
```

Khi dùng biến kiểu `Animal` tham chiếu tới `Dog`, lời gọi `Speak()` sẽ chạy phiên bản `Dog`. Đây là đa hình runtime.

## Câu 4. Thành viên static và object

Thành viên `static` thuộc về **class**, không thuộc về từng object. Vì vậy không cần tạo object bằng `new` để truy cập.

Ví dụ:

```csharp
class Test
{
    public static int Count = 10;
}

Console.WriteLine(Test.Count);
```

Không truy cập theo kiểu `new Test().Count` đối với thành viên static. Cách truy cập đúng là thông qua tên lớp: `Test.Count`.

using Kont.backend.Services;

namespace Kont.backend.tests.Services;

[TestFixture]
public class QrCodeServiceTests
{
    private IQrCodeService _service;

    [SetUp]
    public void Setup()
    {
        _service = new QrCodeService();
    }

    [Test]
    public void GeneratePng_ReturnsValidByteArray()
    {
        var testUrl = "https://example.com/test";
        
        var result = _service.GeneratePng(testUrl);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
        
        // Verify it's a valid PNG by checking the PNG signature
        Assert.That(result[0], Is.EqualTo(0x89));
        Assert.That(result[1], Is.EqualTo(0x50));
        Assert.That(result[2], Is.EqualTo(0x4E));
        Assert.That(result[3], Is.EqualTo(0x47));
    }

    [Test]
    public void GenerateBase64Png_ReturnsValidBase64String()
    {
        var testUrl = "https://example.com/test";
        
        var result = _service.GenerateBase64Png(testUrl);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
        
        // Verify it's valid base64
        var bytes = Convert.FromBase64String(result);
        Assert.That(bytes.Length, Is.GreaterThan(0));
        
        // Verify it's a valid PNG
        Assert.That(bytes[0], Is.EqualTo(0x89));
        Assert.That(bytes[1], Is.EqualTo(0x50));
        Assert.That(bytes[2], Is.EqualTo(0x4E));
        Assert.That(bytes[3], Is.EqualTo(0x47));
    }

    [Test]
    public void GeneratePng_WithCustomPixelsPerModule_ReturnsDifferentSize()
    {
        var testUrl = "https://example.com/test";
        
        var smallQr = _service.GeneratePng(testUrl, 4);
        var largeQr = _service.GeneratePng(testUrl, 12);
        
        Assert.That(smallQr.Length, Is.Not.EqualTo(largeQr.Length));
        Assert.That(smallQr.Length, Is.LessThan(largeQr.Length));
    }
}

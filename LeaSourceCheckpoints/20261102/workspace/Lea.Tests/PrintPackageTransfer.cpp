#define _WIN32_WINNT 0x0602
#define NTDDI_VERSION 0x06020000
#include <windows.h>
#include <initguid.h>
#include <documenttarget.h>
#include <xpsobjectmodel_1.h>
#include <shlwapi.h>
#include <wrl/client.h>
#include <math.h>
#include <stdio.h>
#include <vector>
using Microsoft::WRL::ComPtr;

#define CHECK_XPS(call) do { HRESULT hr_ = (call); if (FAILED(hr_)) return hr_; } while (0)
static const UINT32 ByteLimit = 4 * 1024 * 1024;
struct PreparedPage {
    ComPtr<IXpsOMPage> page;
    ComPtr<IXpsOMPrintTicketResource> ticket;
    ComPtr<IXpsOMStoryFragmentsResource> story;
    ComPtr<IXpsOMImageResource> thumbnail;
    XPS_SIZE size;
};
struct PreparedDocument {
    ComPtr<IOpcPartUri> name;
    ComPtr<IXpsOMPrintTicketResource> ticket;
    ComPtr<IXpsOMDocumentStructureResource> structure;
    std::vector<PreparedPage> pages;
};
struct PreparedPackage {
    ComPtr<IXpsOMPackage> package;
    ComPtr<IOpcPartUri> name;
    std::vector<PreparedDocument> documents;
    UINT32 pages = 0;
};

// Trusted Lea-generated fixture input only, not an untrusted XPS importer.
static HRESULT Prepare(IXpsOMObjectFactory* factory, const BYTE* bytes, UINT32 length, PreparedPackage& result)
{
    if (!factory || !bytes || !length || length > ByteLimit) return E_INVALIDARG;
    ComPtr<IStream> input;
    input.Attach(SHCreateMemStream(bytes, length));
    if (!input) return E_OUTOFMEMORY;
    CHECK_XPS(factory->CreatePackageFromStream(input.Get(), FALSE, &result.package));
    ComPtr<IXpsOMDocumentSequence> sequence;
    CHECK_XPS(result.package->GetDocumentSequence(&sequence));
    CHECK_XPS(sequence->GetPartName(&result.name));
    ComPtr<IXpsOMPrintTicketResource> sequenceTicket;
    CHECK_XPS(sequence->GetPrintTicketResource(&sequenceTicket));
    // Job-level tickets must be supplied at target creation; never silently drop one.
    if (sequenceTicket) return E_NOTIMPL;
    ComPtr<IXpsOMDocumentCollection> documents;
    CHECK_XPS(sequence->GetDocuments(&documents));
    UINT32 count = 0;
    CHECK_XPS(documents->GetCount(&count));
    if (!count || count > 256) return E_INVALIDARG;
    for (UINT32 d = 0; d < count; ++d) {
        ComPtr<IXpsOMDocument> document;
        CHECK_XPS(documents->GetAt(d, &document));
        PreparedDocument prepared;
        CHECK_XPS(document->GetPartName(&prepared.name));
        CHECK_XPS(document->GetPrintTicketResource(&prepared.ticket));
        CHECK_XPS(document->GetDocumentStructureResource(&prepared.structure));
        ComPtr<IXpsOMSignatureBlockResourceCollection> signatures;
        CHECK_XPS(document->GetSignatureBlockResources(&signatures));
        UINT32 signatureCount = 0;
        if (signatures) CHECK_XPS(signatures->GetCount(&signatureCount));
        if (signatureCount) return E_NOTIMPL;
        ComPtr<IXpsOMPageReferenceCollection> references;
        CHECK_XPS(document->GetPageReferences(&references));
        UINT32 pages = 0;
        CHECK_XPS(references->GetCount(&pages));
        if (!pages || pages > 256 - result.pages) return E_INVALIDARG;
        for (UINT32 p = 0; p < pages; ++p) {
            ComPtr<IXpsOMPageReference> reference;
            CHECK_XPS(references->GetAt(p, &reference));
            BOOL restricted = FALSE;
            CHECK_XPS(reference->HasRestrictedFonts(&restricted));
            if (restricted) return E_NOTIMPL;
            PreparedPage page;
            CHECK_XPS(reference->GetPage(&page.page));
            CHECK_XPS(page.page->GetPageDimensions(&page.size));
            if (!isfinite(page.size.width) || !isfinite(page.size.height) ||
                page.size.width <= 0 || page.size.height <= 0) return E_INVALIDARG;
            CHECK_XPS(reference->GetPrintTicketResource(&page.ticket));
            CHECK_XPS(reference->GetStoryFragmentsResource(&page.story));
            CHECK_XPS(reference->GetThumbnailResource(&page.thumbnail));
            prepared.pages.push_back(page);
        }
        result.pages += pages;
        result.documents.push_back(prepared);
    }
    return S_OK;
}

extern "C" __declspec(dllexport) HRESULT __stdcall LeaWriteXps(
    IUnknown* originalTarget, const BYTE* bytes, UINT32 length, UINT32* writtenPages)
{
    if (!writtenPages) return E_POINTER;
    *writtenPages = 0;
    if (!originalTarget || !bytes || !length || length > ByteLimit) return E_INVALIDARG;
    try {
        ComPtr<IPrintDocumentPackageTarget> target;
        CHECK_XPS(originalTarget->QueryInterface(IID_PPV_ARGS(&target)));
        ComPtr<IXpsDocumentPackageTarget> xps;
        CHECK_XPS(target->GetPackageTarget(ID_DOCUMENTPACKAGETARGET_MSXPS, IID_PPV_ARGS(&xps)));
        ComPtr<IXpsOMObjectFactory> factory;
        CHECK_XPS(xps->GetXpsOMFactory(&factory));
        PreparedPackage prepared;
        CHECK_XPS(Prepare(factory.Get(), bytes, length, prepared));
        ComPtr<IOpcPartUri> discard;
        CHECK_XPS(factory->CreatePartUri(L"/LeaDiscardControl.xml", &discard));
        ComPtr<IXpsOMPackageWriter> writer;
        CHECK_XPS(xps->GetXpsOMPackageWriter(prepared.name.Get(), discard.Get(), &writer));
        for (const auto& document : prepared.documents) {
            CHECK_XPS(writer->StartNewDocument(document.name.Get(), document.ticket.Get(),
                document.structure.Get(), nullptr, nullptr));
            for (const auto& page : document.pages) {
                CHECK_XPS(writer->AddPage(page.page.Get(), &page.size, nullptr,
                    page.story.Get(), page.ticket.Get(), page.thumbnail.Get()));
                ++*writtenPages;
            }
        }
        // The caller still owns target cancellation and terminal notification on every failure.
        return writer->Close();
    } catch (const std::bad_alloc&) { return E_OUTOFMEMORY; }
      catch (...) { return E_UNEXPECTED; }
}

#ifdef LEA_TRANSFER_PROBE
int wmain(int argc, wchar_t** argv)
{
    if (argc != 2) return 2;
    UINT32 pages = 999;
    BYTE invalid[] = { 1, 2, 3 };
    if (LeaWriteXps(nullptr, invalid, 3, nullptr) != E_POINTER ||
        LeaWriteXps(nullptr, invalid, 3, &pages) != E_INVALIDARG || pages != 0) return 3;
    HRESULT initialized = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    if (FAILED(initialized)) return 4;
    HRESULT status = E_FAIL;
    {
        ComPtr<IXpsOMObjectFactory> factory;
        status = CoCreateInstance(__uuidof(XpsOMObjectFactory), nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&factory));
        if (SUCCEEDED(status)) {
            FILE* file = nullptr;
            if (_wfopen_s(&file, argv[1], L"rb") == 0 && file) {
                std::vector<BYTE> input(ByteLimit + 1);
                size_t length = fread(input.data(), 1, input.size(), file);
                bool readFailed = ferror(file) != 0;
                fclose(file);
                if (!readFailed && length && length <= ByteLimit) {
                    PreparedPackage malformed;
                    HRESULT rejected = Prepare(factory.Get(), invalid, 3, malformed);
                    if (SUCCEEDED(rejected)) status = E_UNEXPECTED;
                    else {
                        PreparedPackage prepared;
                        status = Prepare(factory.Get(), input.data(), static_cast<UINT32>(length), prepared);
                        if (SUCCEEDED(status)) printf("{\"passed\":true,\"pages\":%u,\"documents\":%u,\"negativeChecks\":3,\"printTargetCreated\":false}\n",
                            prepared.pages, static_cast<unsigned>(prepared.documents.size()));
                    }
                } else status = E_INVALIDARG;
            } else status = HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND);
        }
    }
    CoUninitialize();
    if (FAILED(status)) fprintf(stderr, "XPS preflight HRESULT 0x%08lX\n", static_cast<unsigned long>(status));
    return FAILED(status) ? 1 : 0;
}
#endif

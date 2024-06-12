﻿using Sustainsys.Saml2.Samlp;
using Sustainsys.Saml2.Serialization;
using Sustainsys.Saml2.Tests.Helpers;
using Sustainsys.Saml2.Xml;
using System.Xml;

namespace Sustainsys.Saml2.Tests.Serialization;
partial class SamlXmlReaderTests
{
    [Fact]
    public void ReadAuthnRequest_Mandatory()
    {
        var source = GetXmlTraverser();

        var subject = new SamlXmlReader();

        var actual = subject.ReadAuthnRequest(source);

        var expected = new AuthnRequest
        {
            Id = "x123",
            IssueInstant = new DateTime(2023, 11, 24, 22, 44, 14, DateTimeKind.Utc),
            Version = "2.0"
        };

        actual.Should().BeEquivalentTo(expected);
    }

    // TODO: Test for missing mandatory

    // Test that an AuthnRequest with optional content present can be read. Start with the most common,
    // add more later.
    [Fact]
    public void ReadAuthnRequest_CanReadOptional()
    {
        var source = GetXmlTraverser();
        ((XmlElement)source.CurrentNode!).Sign(
            TestData.Certificate, source.CurrentNode![Constants.Elements.Issuer, Constants.Namespaces.SamlUri]!);

        var subject = new SamlXmlReader();

        var actual = subject.ReadAuthnRequest(source);

        var expected = new AuthnRequest
        {
            Id = "x123",
            IssueInstant = new DateTime(2023, 11, 24, 22, 44, 14, DateTimeKind.Utc),
            Version = "2.0",
            Destination = "https://idp.example.com/Sso",
            Consent = "urn:oasis:names:tc:SAML:2.0:consent:obtained",
            Issuer = "https://sp.example.com/Metadata",
            Extensions = new(),
            Subject = new()
            {
                NameId = "abc12345"
            },
            // TODO: Add NameIDPolicy
            // TODO: Add Conditions
            // TODO: Add RequestedAuthnContext
            // TODO: Add Scoping
            ForceAuthn = true,
            IsPassive = true,
            AssertionConsumerServiceUrl = "https://sp.example.com/Acs",
            AssertionConsumerServiceIndex = 42,
            ProtocolBinding = Constants.BindingUris.HttpPOST,
            // TODO: Add AttributeConsumingServiceIndex
            // TODO: Add ProviderName
        };

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadAuthnRequest_ErrorCallback()
    {
        var source = GetXmlTraverser(nameof(ReadAuthnRequest_Error));

        var subject = new SamlXmlReader();

        bool errorInspectorCalled = false;

        void errorInspector(ReadErrorInspectorContext<AuthnRequest> context)
        {
            context.Data.Id.Should().Be("x123");

            var xmlSourceElement = context.XmlSource as XmlElement;
            xmlSourceElement.Should().NotBeNull();
            xmlSourceElement!.GetAttribute("ID").Should().Be("x123");

            context.Errors.Count.Should().Be(1);
            var error = context.Errors.Single();
            error.Node.Should().BeSameAs(context.XmlSource);
            error.LocalName.Should().Be("Version");
            error.Reason.Should().Be(ErrorReason.MissingAttribute);
            error.Ignore.Should().BeFalse();

            error.Ignore = true;

            errorInspectorCalled = true;
        }

        subject.ReadAuthnRequest(source, errorInspector);

        errorInspectorCalled.Should().BeTrue();
    }

    [Fact]
    public void ReadAuthnRequest_Error()
    {
        var source = GetXmlTraverser();

        var subject = new SamlXmlReader();

        subject.Invoking(s => s.ReadAuthnRequest(source))
            .Should().Throw<SamlXmlException>()
            .WithMessage("*Version*not found*");
    }
}

namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core
open Fable.Core.JsInterop
open Shared
open Feliz.Router

type PrivacyPolicy =
    static member Main() =
        let effectiveDate = "2024-05-14"
        let websiteUrl = Shared.Urls.WebsiteUrl
        let contactEmail = Shared.Urls.ContactEmailObfuscated
        Components.MainCard.Main [
            Html.div [
                prop.className "prose"
                prop.children [
                    Html.h1 "Privacy Policy"
                    Html.p $"Effective Date: {effectiveDate}"
                    Html.p $"Thank you for using our machine learning web service. Your privacy is important to us. This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you use our website located at {websiteUrl} and our machine learning service."
                    Html.p [
                        Html.text $"If you have any questions or concerns about our privacy practices, please contact us via "
                        Html.a [prop.href contactEmail; prop.text "email"]
                    ]

                    Html.h2 "Information We Collect"

                    Html.p "We do not collect any personally identifiable information about you unless you voluntarily provide it to us. When you use our machine learning web service, we may collect the following types of information:"

                    Html.ul [
                        Html.li "Data Processing Information: When you submit data for processing, we collect the information necessary to perform the requested analysis. This may include text, images, or other data types relevant to the machine learning task."
                        Html.li "Email Address (Optional): If you opt-in to receive email notifications, we collect your email address to send you updates about your processing status and results."
                    
                    ]

                    Html.h2 "How We Use Your Information"

                    Html.p "We use the information we collect from you to provide, maintain, and improve our machine learning web service. Specifically, we may use your information to:"

                    Html.ul [
                        Html.li "Process the data you submit through our service."
                        Html.li "Send you email notifications regarding the status of your data processing."
                        Html.li "Provide you with access to download your processed results."
                        Html.li "Ensure the security of our website and prevent fraud."
                        Html.li "Comply with legal obligations."
                    ]

                    Html.h2 "Data Retention"

                    Html.p "We retain your data for a limited period necessary to fulfill the purposes outlined in this Privacy Policy. Specifically, we delete all processed data exactly one week after the processing is completed."

                    Html.h2 "Disclosure of Your Information"

                    Html.p "We do not disclose your information to third parties or make it publicly available. Your data is used solely for the purposes of providing the machine learning service you have requested."

                    Html.h2 "Your Rights"

                    Html.p "You have the right to access, correct, or delete your personal information."

                    Html.h2 "Changes to Our Privacy Policy"

                    Html.p "We reserve the right to update or modify this Privacy Policy at any time and will notify you of any changes by posting the revised policy on our website."

                    Html.h2 "Contact Us"

                    Html.p [
                        Html.text "If you have any questions or concerns about our Privacy Policy, please contact us via ."
                        Html.a [prop.href contactEmail; prop.text "email"]
                    ]
                ]
            ]
        ]
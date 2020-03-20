
function RegisterUiHelper() {
    //ui.authActions.authorize = function (authorization) {
    //    authorization.apiKey.value = "Bearer " + authorization.apiKey.value;
    //    console.log("authorized with bearer token : " + authorization.apiKey.value);
    //    originalAuthorize(authorization);
    //};
}

document.addEventListener("DOMContentLoaded", () => {
        setTimeout(function () { RegisterUiHelper(); }, 1000);
});
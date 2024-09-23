
const breadcrumbCoontainer = document.getElementById("breadcrumb-container");

//funcion para actualizar el breadcrumb
function UpdateBreadcrumb() {
    //limpiar contenido actual
    breadcrumbCoontainer.innerHTML = "";
    //agregar el primer elemento
    breadcrumbCoontainer.innerHTML += `<li class="breadcrumb-item"><a href="/">Home</a></li>`;

    //validar sio el controlador no es home
    if (controllerName!="Home") {

        //mostrar el nombre del controlador
        breadcrumbCoontainer.innerHTML +=
            `<li class="breadcrumb-item"><a href="/${controllerName}">${controllerName}</a></li>`
    }
    //mostrar el nombre del metodo 
    breadcrumbCoontainer.innerHTML += `<li class="breadcrumb-item active" aria-current="page">${actionName}</li>`;


}

//llamar la funcion al cargar la pagina
UpdateBreadcrumb();

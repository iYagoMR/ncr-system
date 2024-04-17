
// Get the images
var images = document.querySelectorAll(".img-fluid");

images.forEach(function (image) {

    //Get the image clicked
    image.onclick = function () {
        let imageRow = image.closest(".row");

        image.classList.toggle("expanded");
        imageRow.classList.toggle("image-modal");
        imageRow.style.display = "flex";

        //Toggle id
        if (image.id === "clickedImage") {
            image.removeAttribute("id");
        } else {
            image.id = "clickedImage";
        }

        // Hide images other than the clicked one
        images.forEach(function (otherImage) {
            if (otherImage !== image) {
                otherImage.classList.toggle("hide-other-imgs")
            }
        });
    }
});





/* Add click event listener*/
//img.addEventListener("click", function () {
//    img.classList.add("expanded")
//});

///* Get the image and insert it inside the modal*/
//var images = document.querySelectorAll(".img-fluid");
//var modalImg = document.getElementById("expandedImg");
//images.forEach(function (image) {
//    image.onclick = function () {
//        let imageSection = this.closest(".accordion-collapse");

//        if (imageSection.classList == "show") {
//            modal.style.display = "block";
//            modalImg.src = this.src;
//        }
//    }

//});


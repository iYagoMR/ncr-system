function addLink(containerId) {
    var linksListContainer = document.getElementById(containerId);
    var linkInput = document.getElementById("Links");
    var linkValue = linkInput.value.trim();

    if (linkValue !== "") {


        // Create a list item to display the link
        var listItem = document.createElement("div");
        var listItemText = document.createElement("p");
        listItemText.textContent = linkValue;
        listItemText.classList.add("standard-border-bottom");

        // Create a hidden input for the link
        var hiddenInput = document.createElement("input");
        hiddenInput.type = "hidden";
        hiddenInput.name = "links";
        hiddenInput.value = linkValue;

        // Add a button to remove the link and its hidden input
        var removeButton = document.createElement("button");
        removeButton.textContent = "Remove";

        removeButton.onclick = function () {
            linksListContainer.removeChild(listItem);
            linksListContainer.removeChild(hiddenInput);

            if (linksListContainer.children.length === 0) {
                linksListContainer.style.display = "none";
            }
        };

        // Append the remove button to the list item
        listItem.appendChild(removeButton);
        listItem.appendChild(listItemText);

        // Append the list item and hidden input to the container
        linksListContainer.appendChild(listItem);
        linksListContainer.appendChild(hiddenInput);

        // Clear the input field
        linkInput.value = "";

        if (linksListContainer.children.length > 0) {
            linksListContainer.style.display = "flex";
        }
    }

}
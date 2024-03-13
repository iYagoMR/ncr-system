function addLink() {
    var linkInput = document.getElementById("Links");
    var linkValue = linkInput.value.trim();

    if (linkValue !== "") {
        // Create a container for the links list
        var linksListContainer = document.getElementById("linksListContainer");

        // Create a list item to display the link
        var listItem = document.createElement("div");
        listItem.textContent = linkValue;

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
        };

        // Append the remove button to the list item
        listItem.appendChild(removeButton);

        // Append the list item and hidden input to the container
        linksListContainer.appendChild(listItem);
        linksListContainer.appendChild(hiddenInput);

        // Clear the input field
        linkInput.value = "";
    }
}
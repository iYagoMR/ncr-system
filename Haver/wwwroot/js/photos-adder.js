//Global Array
var selectedFiles = [];

function handleFileSelection(input, fileContainer) {
    var fileListContainer = document.getElementById(fileContainer);

    // Get the selected files
    var files = input.files;

    // Add the selected files to the global array
    selectedFiles = selectedFiles.concat(Array.from(files));

    // Update the input value with the selected files
    updateInputValue(input);

    // Clear the displayed file list and re-render
    renderFileList(input, fileContainer);

    if (selectedFiles.length > 0) {
        fileListContainer.style.display = "flex";
    }
}

function updateInputValue(input) {
    // Create a new FileList and assign it to the input
    var newFileList = new DataTransfer();
    selectedFiles.forEach(function (f) {
        newFileList.items.add(f);
    });
    input.files = newFileList.files;
}

function renderFileList(input, fileContainer) {
    // Clear the existing file list
    var fileListContainer = document.getElementById(fileContainer);
    fileListContainer.innerHTML = "";

    // Display the selected files
    selectedFiles.forEach(function (file) {
        var listItem = document.createElement("li");
        var listItemText = document.createElement("p");
        listItemText.classList.add("standard-border-bottom")
        listItemText.textContent = file.name;

        // Add a button to remove the file
        var removeButton = document.createElement("button");
        removeButton.textContent = "Remove";
        removeButton.onclick = createRemoveHandler(input, file, fileContainer);

        // Append the remove button to the list item
        listItem.appendChild(removeButton);
        listItem.appendChild(listItemText);

        // Append the list item to the container
        fileListContainer.appendChild(listItem);

        if (selectedFiles.length < 1) {
            fileListContainer.style.display = "none";
        }
    });
}

function createRemoveHandler(input, file, fileContainer) {
    return function (event) {
        event.preventDefault(); // Prevent form submission

        // Remove the file from the input's files array
        var newFiles = Array.from(input.files).filter(function (f) {
            return f !== file;
        });

        // Create a new FileList and assign it to the input
        var newFileList = new DataTransfer();
        newFiles.forEach(function (f) {
            newFileList.items.add(f);
        });
        input.files = newFileList.files;
        selectedFiles = newFiles;

        // Get the container ID
        var fileListContainerId = document.getElementById(fileContainer);

        // Remove the corresponding list item from the DOM
        var listItem = event.target.closest("li");
        listItem.parentNode.removeChild(listItem);
        if (newFiles.length < 1) {
            fileListContainerId.style.display = "none";
        }

    };
}
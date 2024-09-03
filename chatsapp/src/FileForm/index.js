const allowedExtensions = ['pdf', 'doc', 'docx', 'txt'];
const maxSize = 1 * 1024 * 1024; // 1MB in bytes

const FileUpload = ({ callback }) => {

    const handleFileChange = (event) => {
        event.preventDefault();
        const selectedFile = event.target.files[0];
        if (!selectedFile) return;

        const fileExtension = selectedFile.name
                            .split('.').pop()
                            .toLowerCase();
        const fileSize = selectedFile.size;
        let errorMessage = '';

        if (!allowedExtensions.includes(fileExtension)) 
        {
            errorMessage = 'Invalid file type. Only PDF, DOC, DOCX, and TXT files are allowed.';
        } 
        else if (fileSize > maxSize) 
        {
            errorMessage = 'File size must not exceed 1MB.';
        }

        if (errorMessage !== ''){
            alert(errorMessage);
            return;
        }

        callback(selectedFile);
    };

    const handleButtonClick = (e) => {
        e.preventDefault();
        document.getElementById('fileInput').click();
    };

    return (
        <div style={style}>
            <input type="file" 
                    id="fileInput" 
                    data-testid="file-input"
                    hidden={true}
                    onChange={handleFileChange}
            />
            <button onClick={handleButtonClick}>Upload File</button>
        </div>
    );
};

const style = {
    marginLeft: "5px",
}

export default FileUpload;
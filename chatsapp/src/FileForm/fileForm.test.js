import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import FileUpload from './index.js';

describe('FileUpload', () => {
  const mockCallback = jest.fn();

  test('renders file upload button', () => {
    render(<FileUpload callback={mockCallback} />);

    const uploadButton = screen.getByText('Upload File');
    expect(uploadButton).toBeInTheDocument();
  });

  test('calls callback with selected file when file is uploaded', () => {
    render(<FileUpload callback={mockCallback} />);

    const file = new File(['file contents'], 'test.pdf', { type: 'application/pdf' });
    const fileInput = screen.getByTestId('file-input');
    fireEvent.change(fileInput, { target: { files: [file] } });

    expect(mockCallback).toHaveBeenCalledWith(file);
  });

  test('calls callback with not allowed selected file when file is uploaded', () => {
    render(<FileUpload callback={mockCallback} />);

    const file = new File(['file contents'], 'test.zip', { type: 'application/zip' });
    const fileInput = screen.getByTestId('file-input');
    fireEvent.change(fileInput, { target: { files: [file] } });

    expect(mockCallback).toHaveBeenCalledTimes(0);
  });

  test('clicks file input when upload button is clicked', () => {
    render(<FileUpload callback={mockCallback} />);

    const uploadButton = screen.getByText('Upload File');
    fireEvent.click(uploadButton);

    const fileInput = screen.getByTestId('file-input');
    expect(fileInput).toHaveAttribute('type', 'file');
  });
});
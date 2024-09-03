import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import Background from './background.png'; // Import your background image path here
import Home from './index.js';

describe('Home', () => {
  test('renders background image with correct attributes and styling', () => {
    render(<Home />);
    
    const imgElement = screen.getByAltText('This is img');

    // Check if the image is rendered with correct source and alt text
    expect(imgElement).toBeInTheDocument();
    expect(imgElement).toHaveAttribute('src', Background);
  });
});
import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Container, Row, Col, Card, Button } from 'react-bootstrap';
import '../styles/Categories.css';
import { Link } from 'react-router-dom';
import { Products } from '../components/Products';
import { urlConfig } from '../configs/UrlConfig';

function Categories() {
  const [activeCategory, setActiveCategory] = useState('Electronics');
  const [index, setIndex] = useState(0);
  const [serverCategories, setServerCategories] = useState([]);

  const fallbackCategories = ['Electronics', 'Appliances', 'Furnitures', 'H/W Tools', 'Events/Decors', 'Sports/Gears'];
  const fallbackCategoryIds = [1, 2, 3, 4, 5, 6];
  const categories = serverCategories.length > 0 ? serverCategories.map((cat) => cat.categoryName) : fallbackCategories;
  const category_ids = serverCategories.length > 0 ? serverCategories.map((cat) => cat.categoryId) : fallbackCategoryIds;

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const response = await axios.get(`${urlConfig.APP_URL}:${urlConfig.APP_PORT}/api/category/getall`);
        if (Array.isArray(response.data) && response.data.length > 0) {
          setServerCategories(response.data);
          setActiveCategory(response.data[0].categoryName);
          setIndex(0);
        }
      } catch (error) {
        console.error('Error loading categories:', error);
      }
    };

    fetchCategories();
  }, []);

  return (
    <div className="app">
      {/* <TopBar />
      <MainNavbar /> */}
      
      <div className="categories-banner">
        <h1>CATEGORIES</h1>
      </div>

      <div className="categories-page">
        <Container fluid="lg">
          {/* Category buttons */}
          <div className="category-buttons">
            {categories.map((category, index1) => (
              <Button
                key={category}
                variant={activeCategory === category ? 'primary' : 'outline-primary'}
                className="category-button"
                onClick={() => {setActiveCategory(category); setIndex(index1)}}
              >
                {category}
              </Button>
            ))}
          </div>

          {/* Products Grid */}
          <Row className="g-4 mt-3">
            <Products categoryId={category_ids[index]} />
          </Row>
        </Container>
      </div>

      {/* <Footer /> */}
    </div>
  );
}

export default Categories;

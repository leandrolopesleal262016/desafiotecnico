document.addEventListener('DOMContentLoaded', () => {
    // Variáveis globais
    const API_URL = ' http://localhost:5015/api'; 
    // const API_URL = 'http://localhost:8080/api';
    let currentPage = 1;
    let pageSize = 12;
    let currentBookId = null;
    
    // Elementos DOM
    const booksContainer = document.getElementById('booksContainer');
    const loadingSpinner = document.getElementById('loadingSpinner');
    const paginationElement = document.getElementById('pagination');
    const searchInput = document.getElementById('searchInput');
    const searchButton = document.getElementById('searchButton');
    const bookForm = document.getElementById('bookForm');
    const saveBookBtn = document.getElementById('saveBookBtn');
    const modalTitle = document.getElementById('bookModalLabel');
    const bookModal = new bootstrap.Modal(document.getElementById('bookModal'));
    const detailsModal = new bootstrap.Modal(document.getElementById('detailsModal'));
    const editBookBtn = document.getElementById('editBookBtn');
    const deleteBookBtn = document.getElementById('deleteBookBtn');
    const toastNotification = document.getElementById('toastNotification');
    const toast = new bootstrap.Toast(toastNotification);
    
    // Inicialização
    loadBooks();
    
    // Event Listeners
    searchButton.addEventListener('click', () => {
        currentPage = 1;
        loadBooks();
    });
    
    searchInput.addEventListener('keyup', (e) => {
        if (e.key === 'Enter') {
            currentPage = 1;
            loadBooks();
        }
    });
    
    document.getElementById('addBookBtn').addEventListener('click', () => {
        resetBookForm();
        modalTitle.textContent = 'Adicionar Livro';
        currentBookId = null;
    });
    
    saveBookBtn.addEventListener('click', saveBook);
    
    editBookBtn.addEventListener('click', () => {
        detailsModal.hide();
        setTimeout(() => {
            modalTitle.textContent = 'Editar Livro';
            document.getElementById('bookId').value = currentBookId;
            
            const title = document.getElementById('detailTitle').textContent;
            const category = document.getElementById('detailCategory').textContent;
            const price = document.getElementById('detailPrice').textContent.replace('R$ ', '');
            const stock = document.getElementById('detailStock').textContent;
            const rating = document.getElementById('detailRating').getAttribute('data-rating');
            const image = document.getElementById('detailImage').src;
            
            document.getElementById('titulo').value = title;
            document.getElementById('categoria').value = category;
            document.getElementById('preco').value = price;
            document.getElementById('estoque').value = stock;
            document.getElementById('avaliacao').value = rating;
            document.getElementById('imagemUrl').value = image;
            
            bookModal.show();
        }, 500);
    });
    
    deleteBookBtn.addEventListener('click', deleteBook);
    
    // Funções
    async function loadBooks() {
        try {
            showLoading(true);
            
            const searchTerm = searchInput.value.trim();
            let url = `${API_URL}/livros?pageNumber=${currentPage}&pageSize=${pageSize}`;
            
            if (searchTerm) {
                // Simplificado para esse exemplo - uma busca mais robusta seria implementada na API
                url += `&search=${encodeURIComponent(searchTerm)}`;
            }
            
            const response = await fetch(url);
            
            if (!response.ok) {
                throw new Error(`Erro ao carregar livros: ${response.status}`);
            }
            
            const data = await response.json();
            displayBooks(data.items);
            setupPagination(data);
        } catch (error) {
            console.error('Erro:', error);
            showNotification('Erro ao carregar livros', 'bg-danger');
        } finally {
            showLoading(false);
        }
    }
    
    function displayBooks(books) {
        booksContainer.innerHTML = '';
        
        if (books.length === 0) {
            booksContainer.innerHTML = `
                <div class="col-12 text-center my-5">
                    <h3>Nenhum livro encontrado</h3>
                    <p>Tente ajustar sua busca ou adicionar novos livros.</p>
                </div>
            `;
            return;
        }
        
        books.forEach(book => {
            const bookElement = createBookElement(book);
            booksContainer.appendChild(bookElement);
        });
    }
    
    function createBookElement(book) {
        const colDiv = document.createElement('div');
        colDiv.className = 'col-md-3 col-sm-6 mb-4';
        
        const starsHtml = generateStarsHtml(book.avaliacao);
        
        colDiv.innerHTML = `
            <div class="book-card">
                <div class="book-image-container">
                    <img src="${book.imagemUrl || 'https://via.placeholder.com/150x200?text=Sem+Imagem'}" 
                         alt="${book.titulo}" class="book-image">
                </div>
                <div class="book-details">
                    <h5 class="book-title">${book.titulo}</h5>
                    <div class="book-rating">
                        ${starsHtml}
                    </div>
                    <div class="book-category">${book.categoria || 'Sem categoria'}</div>
                    <p class="book-price">R$ ${book.preco.toFixed(2)}</p>
                    <div class="book-actions">
                        <button class="btn btn-sm btn-primary view-details" data-id="${book.id}">
                            Ver Detalhes
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        // Adicionar event listener para o botão de detalhes
        colDiv.querySelector('.view-details').addEventListener('click', () => {
            showBookDetails(book.id);
        });
        
        return colDiv;
    }
    
    function generateStarsHtml(rating) {
        let starsHtml = '';
        for (let i = 1; i <= 5; i++) {
            if (i <= rating) {
                starsHtml += '<i class="fas fa-star checked"></i>';
            } else {
                starsHtml += '<i class="far fa-star"></i>';
            }
        }
        return starsHtml;
    }
    
    function setupPagination(data) {
        paginationElement.innerHTML = '';
        
        if (data.totalPages <= 1) {
            return;
        }
        
        // Botão Anterior
        const prevLi = document.createElement('li');
        prevLi.className = `page-item ${!data.hasPreviousPage ? 'disabled' : ''}`;
        prevLi.innerHTML = `
            <a class="page-link" href="#" aria-label="Anterior">
                <span aria-hidden="true">&laquo;</span>
            </a>
        `;
        
        if (data.hasPreviousPage) {
            prevLi.addEventListener('click', (e) => {
                e.preventDefault();
                currentPage--;
                loadBooks();
            });
        }
        
        paginationElement.appendChild(prevLi);
        
        // Números das páginas
        const startPage = Math.max(1, currentPage - 2);
        const endPage = Math.min(data.totalPages, currentPage + 2);
        
        for (let i = startPage; i <= endPage; i++) {
            const pageLi = document.createElement('li');
            pageLi.className = `page-item ${i === currentPage ? 'active' : ''}`;
            pageLi.innerHTML = `<a class="page-link" href="#">${i}</a>`;
            
            pageLi.addEventListener('click', (e) => {
                e.preventDefault();
                currentPage = i;
                loadBooks();
            });
            
            paginationElement.appendChild(pageLi);
        }
        
        // Botão Próximo
        const nextLi = document.createElement('li');
        nextLi.className = `page-item ${!data.hasNextPage ? 'disabled' : ''}`;
        nextLi.innerHTML = `
            <a class="page-link" href="#" aria-label="Próximo">
                <span aria-hidden="true">&raquo;</span>
            </a>
        `;
        
        if (data.hasNextPage) {
            nextLi.addEventListener('click', (e) => {
                e.preventDefault();
                currentPage++;
                loadBooks();
            });
        }
        
        paginationElement.appendChild(nextLi);
    }
    
    async function showBookDetails(id) {
        try {
            showLoading(true);
            
            const response = await fetch(`${API_URL}/livros/${id}`);
            
            if (!response.ok) {
                throw new Error(`Erro ao carregar detalhes do livro: ${response.status}`);
            }
            
            const book = await response.json();
            currentBookId = book.id;
            
            // Preencher o modal de detalhes
            document.getElementById('detailTitle').textContent = book.titulo;
            document.getElementById('detailCategory').textContent = book.categoria || 'Sem categoria';
            document.getElementById('detailPrice').textContent = `R$ ${book.preco.toFixed(2)}`;
            document.getElementById('detailStock').textContent = book.estoque || 'Não informado';
            
            const ratingElem = document.getElementById('detailRating');
            ratingElem.innerHTML = generateStarsHtml(book.avaliacao);
            ratingElem.setAttribute('data-rating', book.avaliacao);
            
            document.getElementById('detailImage').src = book.imagemUrl || 'https://via.placeholder.com/300x400?text=Sem+Imagem';
            
            detailsModal.show();
        } catch (error) {
            console.error('Erro:', error);
            showNotification('Erro ao carregar detalhes do livro', 'bg-danger');
        } finally {
            showLoading(false);
        }
    }
    
    async function saveBook() {
        try {
            if (!bookForm.checkValidity()) {
                bookForm.reportValidity();
                return;
            }
            
            const bookData = {
                titulo: document.getElementById('titulo').value,
                preco: parseFloat(document.getElementById('preco').value),
                estoque: document.getElementById('estoque').value,
                avaliacao: parseInt(document.getElementById('avaliacao').value),
                imagemUrl: document.getElementById('imagemUrl').value,
                categoria: document.getElementById('categoria').value
            };
            
            const id = document.getElementById('bookId').value;
            const isEditing = id && id.trim() !== '';
            
            const url = isEditing ? `${API_URL}/livros/${id}` : `${API_URL}/livros`;
            const method = isEditing ? 'PUT' : 'POST';
            
            const response = await fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(bookData)
            });
            
            if (!response.ok) {
                throw new Error(`Erro ao ${isEditing ? 'atualizar' : 'adicionar'} livro: ${response.status}`);
            }
            
            bookModal.hide();
            
            showNotification(
                `Livro ${isEditing ? 'atualizado' : 'adicionado'} com sucesso!`, 
                'bg-success'
            );
            
            loadBooks();
        } catch (error) {
            console.error('Erro:', error);
            showNotification(`Erro ao ${currentBookId ? 'atualizar' : 'adicionar'} livro`, 'bg-danger');
        }
    }
    
    async function deleteBook() {
        if (!currentBookId) return;
        
        if (!confirm('Tem certeza que deseja excluir este livro?')) {
            return;
        }
        
        try {
            const response = await fetch(`${API_URL}/livros/${currentBookId}`, {
                method: 'DELETE'
            });
            
            if (!response.ok) {
                throw new Error(`Erro ao excluir livro: ${response.status}`);
            }
            
            detailsModal.hide();
            showNotification('Livro excluído com sucesso!', 'bg-success');
            loadBooks();
        } catch (error) {
            console.error('Erro:', error);
            showNotification('Erro ao excluir livro', 'bg-danger');
        }
    }
    
    function resetBookForm() {
        bookForm.reset();
        document.getElementById('bookId').value = '';
    }
    
    function showLoading(show) {
        loadingSpinner.style.display = show ? 'flex' : 'none';
        booksContainer.style.display = show ? 'none' : 'flex';
    }
    
    function showNotification(message, bgClass) {
        const toastBody = document.getElementById('toastMessage');
        toastBody.textContent = message;
        
        // Remover classes de cores anteriores
        toastNotification.classList.remove('bg-success', 'bg-danger', 'bg-warning', 'bg-info');
        
        // Adicionar a classe de cor apropriada
        if (bgClass) {
            toastNotification.classList.add(bgClass);
        }
        
        toast.show();
    }
});
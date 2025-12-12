using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<UserCard> UserCards => Set<UserCard>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<TradeOffer> TradeOffers => Set<TradeOffer>();
    public DbSet<TradeItem> TradeItems => Set<TradeItem>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ProductReport> ProductReports => Set<ProductReport>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // A) Enum -> string (CHECK’lere birebir yaklaþmak için)
        b.Entity<Product>().Property(x => x.Gender).HasConversion<string>();
        b.Entity<Product>().Property(x => x.Status).HasConversion<string>();
        b.Entity<Coupon>().Property(x => x.DiscountType).HasConversion<string>();
        b.Entity<Order>().Property(x => x.PaymentMethod).HasConversion<string>();
        b.Entity<Order>().Property(x => x.Status).HasConversion<string>();
        b.Entity<TradeOffer>().Property(x => x.Status).HasConversion<string>();
        b.Entity<TradeItem>().Property(x => x.ItemType).HasConversion<string>();

        // B) Parent-Child delete davranýþý (SQL’de parent_id FK var ama davranýþ belirtilmemiþ) :contentReference[oaicite:21]{index=21}
        b.Entity<Category>()
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // C) SQL’de ON DELETE CASCADE olanlar :contentReference[oaicite:22]{index=22}
        b.Entity<UserCard>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProductImage>().HasOne(x => x.Product).WithMany(p => p.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<OrderItem>().HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<TradeItem>().HasOne(x => x.TradeOffer).WithMany(t => t.Items).HasForeignKey(x => x.TradeId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Message>().HasOne(x => x.Conversation).WithMany(c => c.Messages).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProductReport>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);

        // D) User FK’lerde “kullanýcý silinince her þey uçmasýn” diye Restrict (önerilen)
        b.Entity<Product>().HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Order>().HasOne(x => x.Buyer).WithMany().HasForeignKey(x => x.BuyerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<OrderItem>().HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Message>().HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Conversation>().HasOne(x => x.Buyer).WithMany().HasForeignKey(x => x.BuyerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Conversation>().HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TradeOffer>().HasOne(x => x.Offerer).WithMany().HasForeignKey(x => x.OffererId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TradeOffer>().HasOne(x => x.Receiver).WithMany().HasForeignKey(x => x.ReceiverId).OnDelete(DeleteBehavior.Restrict);

        // E) CHECK constraint’ler (SQL’de var) :contentReference[oaicite:23]{index=23}
        b.Entity<Product>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Products_Gender", "Gender IN ('Male','Female','Unisex','Kids')");
            t.HasCheckConstraint("CK_Products_Status", "Status IN ('draft','published','sold','blocked')");
        });

        b.Entity<Coupon>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Coupons_DiscountType", "DiscountType IN ('percentage','fixed')");
        });

        b.Entity<Order>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Orders_PaymentMethod", "PaymentMethod IN ('bank_transfer','cash_on_delivery','credit_card')");
            t.HasCheckConstraint("CK_Orders_Status", "Status IN ('preparing','shipped','delivered','cancelled')");
        });

        b.Entity<TradeOffer>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_TradeOffers_Status", "Status IN ('pending','accepted','rejected','cancelled')");
        });

        b.Entity<TradeItem>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_TradeItems_ItemType", "ItemType IN ('offered','requested')");
        });

        b.Entity<Review>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Reviews_Rating", "Rating >= 1 AND Rating <= 5");
        });
    }
}

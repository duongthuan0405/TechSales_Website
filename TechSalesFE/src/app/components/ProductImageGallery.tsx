import { useEffect, useState } from 'react';
import useEmblaCarousel from 'embla-carousel-react';
import { ChevronLeft, ChevronRight, ImageOff } from 'lucide-react';
import { Product } from '../../models/ui_types/product';
import { Button } from './ui/button';

export function ProductImageGallery({ product }: { product: Product }) {
  const images = Array.from(new Set([
    product.imageUrl,
    ...(product.images ?? []),
    ...(product.imageDetails?.map(image => image.imageUrl) ?? []),
  ].filter((url): url is string => !!url)));
  const [viewportRef, api] = useEmblaCarousel({ loop: images.length > 1 });
  const [selected, setSelected] = useState(0);
  const imageKey = JSON.stringify(images);

  useEffect(() => {
    if (!api) return;
    const updateSelection = () => setSelected(api.selectedScrollSnap());
    api.on('select', updateSelection);
    api.on('reInit', updateSelection);
    updateSelection();
    return () => {
      api.off('select', updateSelection);
      api.off('reInit', updateSelection);
    };
  }, [api]);

  useEffect(() => {
    api?.scrollTo(0, true);
  }, [api, product.id, imageKey]);

  if (!images.length) {
    return <div className="flex aspect-square flex-col items-center justify-center gap-3 rounded-2xl border border-border bg-card text-muted-foreground">
      <ImageOff className="h-10 w-10" aria-hidden="true" />
      <span>Chưa có ảnh sản phẩm</span>
    </div>;
  }

  return <section className="min-w-0 space-y-3" aria-label={`Ảnh sản phẩm ${product.name}`}>
    <div className="relative" role="region" aria-roledescription="carousel" tabIndex={0}
      onKeyDown={event => {
        if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
          event.preventDefault();
          if (event.key === 'ArrowLeft') api?.scrollPrev();
          else api?.scrollNext();
        }
      }}>
      <div ref={viewportRef} className="overflow-hidden rounded-2xl border border-border bg-card">
        <div className="flex touch-pan-y">
          {images.map((url, index) => <div key={url} className="aspect-square min-w-0 shrink-0 basis-full p-10" role="group" aria-label={`Ảnh ${index + 1} / ${images.length}`}>
            <img src={url} alt={`${product.name} - ảnh ${index + 1}`} className="h-full w-full object-contain" draggable={false} />
          </div>)}
        </div>
      </div>
      {images.length > 1 && <>
        <Button type="button" variant="outline" size="icon" className="absolute left-2 top-1/2 -translate-y-1/2 rounded-full bg-card" aria-label="Ảnh trước" onClick={() => api?.scrollPrev()}><ChevronLeft /></Button>
        <Button type="button" variant="outline" size="icon" className="absolute right-2 top-1/2 -translate-y-1/2 rounded-full bg-card" aria-label="Ảnh tiếp theo" onClick={() => api?.scrollNext()}><ChevronRight /></Button>
        <span className="absolute bottom-3 left-1/2 -translate-x-1/2 rounded-full bg-card/90 px-3 py-1 text-xs" aria-live="polite">{selected + 1} / {images.length}</span>
      </>}
    </div>
    {images.length > 1 && <div className="flex gap-2 overflow-x-auto pb-2" aria-label="Chọn ảnh sản phẩm">
      {images.map((url, index) => <button key={url} type="button" aria-label={`Xem ảnh ${index + 1}`} aria-pressed={selected === index}
        onClick={() => api?.scrollTo(index)}
        className={`h-20 w-20 shrink-0 rounded-lg border-2 bg-card p-2 focus-visible:outline focus-visible:outline-2 focus-visible:outline-primary ${selected === index ? 'border-primary' : 'border-border'}`}>
        <img src={url} alt="" className="h-full w-full object-contain" loading="lazy" />
      </button>)}
    </div>}
  </section>;
}

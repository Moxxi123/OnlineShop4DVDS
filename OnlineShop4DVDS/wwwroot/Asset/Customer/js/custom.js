	
 $(document).ready(function(){

		$('.shop-slider').owlCarousel({
	   loop: true,
	   margin:30,
	   autoplay:true,
	   nav:false,
	   dots:false,
	   responsive: {
		   0: {
			   items:1
		   },
		   600: {
			   items:1
		   },
		   667: {
			 items:2
		   },
		   1000: {
			   items:4
		   }
	   }
	})
   
	
$('.slider-testi-20').owlCarousel({
	loop: true,
	margin:30,
	autoplay:true,
	nav:false,
	dots:true,
	responsive: {
	  0: {
		items:1
	  },
	  375: {
		items:1
	  },
	  600: {
		  items:1
	  },
	  667: {
		items:2
	  },
	  1000: {
		  items:2
	  },
	  1024: {
		items:3
	  },
	  1200: {
		items:3
	  }
  
	}
  })

  $('.slidet-logo').owlCarousel({
	loop: true,
	margin:30,
	autoplay:true,
	nav:false,
	dots:true,
	responsive: {
	  0: {
		items:1
	  },
	  375: {
		items:2
	  },
	  600: {
		  items:2
	  },
	  667: {
		items:2
	  },
	  896: {
		  items:3
	  },
	  1024: {
		items:3
	  },
	  1180: {
		items:5
	  },
	  1200: {
		items:7
	  }
  
	}
  })


		$('.slider-div').owlCarousel({
		loop: true,
		margin:30,
		autoplay:true,
		nav:false,
		dots:true,
		responsive: {
			0: {
				items:1
			},
			600: {
				items:1
			},
			667: {
			items:1
			},
			768: {
				items:1
			},
			1000: {
				items:1
			}
		}
	})
  
});








$(document).ready(function(){
	$('.slider-testi').owlCarousel({
	loop: true,
	margin:30,
	autoplay:true,
	nav:false,
	dots:true,
	responsive: {
		0: {
			items:1
		},
		600: {
			items:1
		},
		667: {
		items:1
		},
		1000: {
			items:1
		}
	}
})

});



$(document).ready(function(){
	$('.team-home-01').owlCarousel({
	loop: true,
	margin:30,
	autoplay:true,
	nav:false,
	dots:true,
	responsive: {
		0: {
			items:1
		},
		600: {
			items:1
		},
		667: {
		items:2
		},
		1000: {
			items:4
		}
	}
})

});





$(document).ready(function() {
    $( window ).scroll(function() {
          var height = $(window).scrollTop();
          if(height >= 100) {
              $('header').addClass('fixed-menu');
          } else {
              $('header').removeClass('fixed-menu');
          }
      });
  });



  //  bank js
$(document).ready(function(){
  $("#customRadio1").click(function(){
    $("#ac-2").hide();
  });
  $("#customRadio1").click(function(){
    $("#ac-1").show();
  });
   $("#customRadio2").click(function(){
    $("#ac-1").hide();
  });
  $("#customRadio2").click(function(){
    $("#ac-2").show();
  });
});
